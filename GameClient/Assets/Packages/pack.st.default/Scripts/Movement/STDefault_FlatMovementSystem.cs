using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Stormium.Internal.PlayerLoop;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine.Profiling;
using _ST._Scripts.Internal.ECS.Character;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UOMovementUpdate.Loop)),
     UpdateBefore(typeof(STUpdateOrder.UOMovementUpdate.FixMovement))]
    public class STDefault_FlatMovementSystem : ComponentSystem
    {
        struct MovementGroup
        {
            public EntityArray                                Entities;
            public ComponentDataArray<STDefault_FlatMovement> Components;

            #region Required by STDefault

            public ComponentDataArray<STDefault_EntityInput>    EntityInputs;
            public ComponentDataArray<STDefault_MovementDetail> Details;
            public ComponentDataArray<DWorldPositionData>          Positions;
            public ComponentDataArray<DWorldRotationData>          Rotations;
            public ComponentArray<Rigidbody>                    Rigidbodies;
            public ComponentDataArray<DCharacterData>              Characters;

            [ReadOnly] public ComponentDataArray<DCharacterInformationData>    CharacterInformations;
            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;

            #endregion

            [ReadOnly] public int Length;
        }

        [Inject] MovementGroup           m_Group;
        [Inject] STCharacterManager      m_CharacterManager;
        [Inject] STWorldTransformManager m_TransformManager;

        public static bool USE_EXPERIMENTAL_RIGIDBODY = false;

        [ComputeJobOptimization]
        [STDefault_InitCoordinator.RequireMinimal]
        struct JobSetPosition : IJobProcessComponentData<DCharacterData, STDefault_FlatMovement, DWorldPositionData>
        {
            [ReadOnly]                  public float                                        DeltaTime;
            [ReadOnly]                  public ComponentDataArray<STDefault_MovementDetail> Details;
            [ReadOnly]                  public ComponentDataArray<DWorldRotationData>          Rotations;
            [ReadOnly]                  public ComponentDataArray<DCharacterInformationData>   CharacterInformations;
            [DeallocateOnJobCompletion] public NativeArray<int>                             Counter;

            // Read and write
            public NativeArray<Vector3> Velocities;

            public void Execute(ref DCharacterData character,
                                [ReadOnly] ref STDefault_FlatMovement movementRef,
                                ref DWorldPositionData position)
            {
                var refIndex             = Counter[0];
                var rotation             = Rotations[refIndex];
                var characterInformation = CharacterInformations[refIndex];
                var minWkSpeed           = movementRef.MinWalkSpeed;
                var maxWkSpeed           = movementRef.MaxWalkSpeed;
                var maxWkSpeedInAir      = movementRef.MaxWalkAirSpeed;
                var WkSpeedIps           = movementRef.WalkSpeedIncreasePerSecond;

                var   moveDir      = character.Direction = rotation.Value * Details[refIndex].MovementDirection;
                var   flatVelocity = (Vector2) ((float3) characterInformation.PreviousVelocity).xz;
                float speedLerp;

                var speed         = flatVelocity.magnitude;
                var minSpeed      = speed < minWkSpeed ? minWkSpeed : speed;
                var maxSpeedInAir = speed < maxWkSpeedInAir ? maxWkSpeedInAir : speed;

                if (character.IsGrounded)
                {
                    speedLerp = math.lerp(minSpeed, maxWkSpeed, DeltaTime * WkSpeedIps * speed);
                    speedLerp = math.clamp(speedLerp, minWkSpeed, 100);
                }
                else
                {
                    minSpeed = math.clamp(speed, 0.5f, speed);
                    speedLerp = maxWkSpeedInAir;
                    
                    var mv          = moveDir * maxWkSpeedInAir;
                    var addToSpeed  = (moveDir.magnitude * 4f * DeltaTime);
                    var speedWithMv = minSpeed + addToSpeed;
                    if (speedWithMv <= maxWkSpeedInAir)
                        minSpeed = speedWithMv;
                    else
                    {
                        minSpeed = speedWithMv - math.distance(speedWithMv, maxWkSpeedInAir);
                    }

                    var minmaxSpeed = math.clamp(speed, minSpeed, maxSpeedInAir);

                    if (USE_EXPERIMENTAL_RIGIDBODY)
                    {
                        var rv = Velocities[refIndex];
                        /*if (mv.magnitude <= 0.5f)
                            rv = math.lerp(rv, Vector3.zero, DeltaTime);*/

                        rv += mv * 4.5f * DeltaTime;

                        /*var flatVector = new Vector2(minmaxSpeed, minmaxSpeed).normalized * minmaxSpeed;
                        var normalizedLimit = new float3(flatVector.x + 2.05f, 0, flatVector.y + 2.05f);*/

                        rv.y = 0;
                        rv   = rv.normalized * minmaxSpeed;
                        rv.y = Velocities[refIndex].y;

                        Velocities[refIndex] = rv;
                    }
                    else
                    {
                        var rv = characterInformation.PreviousVelocity;

                        //rv += mv * 4.5f * DeltaTime;

                        //rv.y = 0;
                        //rv   = rv.normalized * minmaxSpeed;

                        position.Value += moveDir * minmaxSpeed * DeltaTime;
                    }
                }

                var result = moveDir * speedLerp * 1f;
  
                if (character.IsGrounded)
                {
                    if (USE_EXPERIMENTAL_RIGIDBODY)
                    {
                        var velocityVec = Velocities[refIndex];
                        if (speed >= minWkSpeed)
                            velocityVec = math.lerp(velocityVec, result, DeltaTime * 32.5f);
                        else
                            velocityVec = result;

                        Velocities[refIndex] = velocityVec;
                    }
                    Velocities[refIndex] = Vector3.zero;
                    
                    position.Value += result * DeltaTime;
                }
                
                character.RunVelocity += result * DeltaTime;

                Counter[0] = refIndex + 1;
            }
        }

        [ComputeJobOptimization]
        [STDefault_InitCoordinator.RequireMinimal]
        struct JobSetPositionRB : IJobProcessComponentData<DCharacterData, STDefault_FlatMovement, DWorldPositionData>
        {
            [ReadOnly]                  public float                                        DeltaTime;
            [ReadOnly]                  public ComponentDataArray<STDefault_MovementDetail> Details;
            [ReadOnly]                  public ComponentDataArray<DWorldRotationData>          Rotations;
            [ReadOnly]                  public ComponentDataArray<DCharacterInformationData>   CharacterInformations;
            [DeallocateOnJobCompletion] public NativeArray<int>                             Counter;

            // Read and write
            public NativeArray<Vector3> Velocities;

            private void SetVelFromFlat(int index, Vector3 flatVector)
            {
                flatVector.y      = Velocities[index].y;
                Velocities[index] = flatVector;
            }

            public unsafe void Execute(ref DCharacterData character,
                                       [ReadOnly] ref STDefault_FlatMovement movementRef,
                                       ref DWorldPositionData position)
            {                
                ref var index    = ref UnsafeUtilityEx.ArrayElementAsRef<int>(Counter.GetUnsafePtr(), 0);
                var     detail   = Details[index];
                var     rotation = Rotations[index];
                var     charInfo = CharacterInformations[index];

                var   flatVelocity = ((float3) charInfo.PreviousVelocity).xz;
                float speed        = length(charInfo.PreviousVelocity), flatSpeed = length(flatVelocity);

                var movementDirection = rotation.Value * detail.MovementDirection;
                var directionSensivity = length(movementDirection);

                if (character.IsGrounded) ;
                {
                    var targetSpeed = clamp(speed + DeltaTime, movementRef.MinWalkSpeed * directionSensivity, movementRef.MaxWalkSpeed);
                    float3 targetVelocity = movementDirection * targetSpeed;

                    var velocity = (float3)Velocities[index];
                    for (var i = 0; i != 2; i++)
                    {
                        if (speed <= movementRef.MinWalkSpeed)
                            velocity[i] = targetVelocity[i];
                    }

                    velocity.y = 0; // Set Y to 0 to normalize it 
                    //velocity = normalize(velocity) * targetSpeed; (WHY ARE WE DOING THAT? WE CAN DO A LERP)
                    //velocity = lerp(velocity, targetVelocity, DeltaTime * movementRef.WalkSpeedIncreasePerSecond);
                    velocity = targetVelocity;
                    //velocity = targetVelocity;
                    velocity.y = Velocities[index].y; // Set back Y

                    Velocities[index] = velocity;
                }

                index++;
            }
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("Things that will be moved");
            //< To move in another file (OOP arena file)
            var deltaTime = Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime;
            //>
            Profiler.EndSample();

            Profiler.BeginSample("Job entry");
            var characterVelocities = new NativeArray<Vector3>(m_Group.Length, Allocator.Temp);
            for (int i = 0; i != m_Group.Length; ++i)
            {
                var rigidbody = m_Group.Rigidbodies[i];
                characterVelocities[i] = rigidbody.velocity;
            }

            new JobSetPosition()
            {
                DeltaTime             = deltaTime,
                Details               = m_Group.Details,
                Velocities            = characterVelocities,
                CharacterInformations = m_Group.CharacterInformations,
                Rotations             = m_Group.Rotations,
                Counter               = new NativeArray<int>(1, Allocator.TempJob)
            }.Run(this);
            Profiler.EndSample();

            Cursor.lockState = CursorLockMode.Locked;

            for (int i = 0; i != m_Group.Length; i++)
            {
                #region Get Variables

                var entity            = m_Group.Entities[i];
                var component         = m_Group.Components[i];
                var transform         = new DWorldTransformData(m_Group.Positions[i].Value, m_Group.Rotations[i].Value);
                var rigidbody         = m_Group.Rigidbodies[i];
                var character         = m_Group.Characters[i];
                var characterCollider = m_Group.CharacterColliders[i];
                var movementCollider  = characterCollider.MovementCollider;
                var colliderPosition  = movementCollider.bounds.center;
                colliderPosition.y = movementCollider.bounds.min.y;

                var rbVelocity  = characterVelocities[i];
                var velY        = rbVelocity.y;
                var newPosition = transform.Position;
                var newRotation = transform.Rotation;
                
                // Do we like... care about velocities?
                //newPosition += rbVelocity * deltaTime;

                #endregion

                #region Apply base positions and rotations

                Profiler.BeginSample("Apply base positions and rotations");
                newRotation.eulerAngles += new Vector3(0, Input.GetAxisRaw("Mouse X") * 0.75f, 0);
                Profiler.EndSample();

                #endregion

                #region Make some raycasts

                Profiler.BeginSample("Make some raycasts");
                var groundHit = m_CharacterManager.UpdateGroundRay(entity.Index, movementCollider as CapsuleCollider, transform.Position,
                    transform.Rotation);
                var isGrounded = character.IsGrounded = groundHit.normal != Vector3.zero
                                                        && groundHit.distance <= 0.045f;
                Profiler.EndSample();

                #endregion

                #region stick to ground

                var isCurrentlyJumping = entity.HasComponent<STDefault_Jump>() &&
                                         entity.GetComponentData<STDefault_Jump>().IsJumping;

                Profiler.BeginSample("stick to ground");
                var operationSuccess = false;
                /*if ((isGrounded || character.WasGrounded) && !isCurrentlyJumping && true == false)
                {
                    var factor = 1f;
                    var minPos =
                        m_CharacterManager.GetMinCenter(movementCollider, transform.Position, transform.Rotation)
                        + new Vector3(0, movementCollider.radius, 0);
                    var maxPos =
                        m_CharacterManager.GetMaxCenter(movementCollider, transform.Position, transform.Rotation)
                        - new Vector3(0, movementCollider.radius, 0);

                    //var raycastHits = Physics.RaycastAll(minPos, Vector3.down,
                    //    characterCollider.FootPanel + 0.025f + (character.MaximumStepAngle * 0.001f) + 1f, m_CharacterManager.LayerMask);
                    var raycastHits = Physics.CapsuleCastAll(minPos, maxPos, movementCollider.radius,
                        Vector3.down,
                        movementCollider.height,
                        m_CharacterManager.LayerMask);

                    var highestImpact = 1f;
                    var length        = raycastHits.Length;
                    for (int hitIndex = 0; hitIndex < length; ++hitIndex)
                    {
                        var raycastHit = raycastHits[hitIndex];
#if GAMEDEBUG
                        Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.green);
    #endif
                        if (raycastHit.normal.y < 0.45f)
                            continue;
                        if (highestImpact < raycastHit.distance)
                            continue;

                        Debug.DrawLine(raycastHit.point, minPos + new Vector3(0, 0.5f, 0), Color.blue, 0.25f);

                        highestImpact = raycastHit.distance;
#if GAMEDEBUG
                        Debug.DrawLine(minPos, raycastHit.point, Color.red, 0.25f);
    #endif
                        newPosition.y    = raycastHit.point.y;
                        isGrounded       = true;
                        operationSuccess = true;
                    }
                }*/

                if ((isGrounded || character.WasGrounded)
                    && groundHit.distance <= characterCollider.FootPanel
                    && groundHit.normal != Vector3.zero
                    && groundHit.normal.y > 0.55f)
                {
                    newPosition.y = groundHit.point.y;
                }

                /*if (!operationSuccess)
                    isGrounded = false;*/

                Profiler.EndSample();

                #endregion

                #region Set ours velocities

                Profiler.BeginSample("Set ours velocities");
                // 'velY' will indicate the Y velocity of the rigidbody, if the first result raycast got a very low distance, we stick to the ground. 
                velY = isGrounded
                    ? 0 + (Physics.gravity.y * deltaTime)
                    : (groundHit.distance < 0.25f ? rbVelocity.y - groundHit.distance : rbVelocity.y);
                // The more time is passed, the more the velocity is zeroed (and it's faster on ground)
                /*rbVelocity = Vector3.Lerp(rbVelocity, Vector3.zero,
                    deltaTime * (10 - math.clamp(groundHit.distance, 0, 10)));*/
                Profiler.EndSample();

                #endregion

                #region Check if we need to zero the XZ (plane) velocity on ground. (flat movement)

                Profiler.BeginSample("Check if we need to zero the XZ (plane) velocity on ground. (flat movement)");
                /*if (rbVelocity.x < 0.01f + character.RunVelocity.x && isGrounded) rbVelocity.x =  0;
                else if (rbVelocity.x < 5 && !isGrounded) rbVelocity.x                                                += character.RunVelocity.x * 0.1f;

                if (rbVelocity.z < 0.01f + character.RunVelocity.z && isGrounded) rbVelocity.z =  0;
                else if (rbVelocity.z < 5 && !isGrounded) rbVelocity.z                                                += character.RunVelocity.z * 0.1f;*/
                Profiler.EndSample();

                //rbVelocity.x = 0;
                //rbVelocity.z = 0;

                #endregion

                #region Apply Constraints

                Profiler.BeginSample("Apply constraints");
                var constraints = 112;

                //if ( /*rbVelocity.x == 0 && */isGrounded) constraints += 2;
                //if ( /*rbVelocity.z == 0 && */isGrounded) constraints += 8;

                rigidbody.constraints = (RigidbodyConstraints) constraints;
                Profiler.EndSample();

                #endregion

                #region Finally set everything

                Profiler.BeginSample("Finally set everything");
                if (isGrounded)
                    velY = 0;
                rbVelocity.y = velY;

                rigidbody.velocity                          = rbVelocity;
                character.IsGrounded                        = isGrounded;
                transform.Position                          = newPosition;
                transform.Rotation                          = newRotation;
                characterCollider.RotateGameObject.rotation = newRotation;

                //m_RigidbodiesManager.UpdateRigidbody(entity, rigidbody);
                m_TransformManager.UpdateTransform(entity, transform);
                m_CharacterManager.UpdateCharacter(entity, character);

                //GameLogs.Log(Time.frameCount + "> Flat System");
                Profiler.EndSample();

                #endregion
            }

            characterVelocities.Dispose();
        }
    }
}
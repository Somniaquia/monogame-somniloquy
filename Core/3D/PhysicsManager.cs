// namespace Somniloquy {
//     using System;
//     using System.Collections.Generic;
//     using System.Numerics;

//     using Microsoft.Xna.Framework;
    
//     using MagicPhysX; // for enable Extension Methods.
//     using static MagicPhysX.NativeMethods; // recommend to use C API.
    
//     public class PhysicsScene {

//         public unsafe PhysicsScene() {
//             var foundation = physx_create_foundation();
//             var physics = physx_create_physics(foundation);
//             var sceneDesc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));
//             sceneDesc.gravity = new PxVec3 { x = 0.0f, y = -9.81f, z = 0.0f };

//             var dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
//             sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
//             sceneDesc.filterShader = get_default_simulation_filter_shader();

//             // create physics scene
//             var scene = physics->CreateSceneMut(&sceneDesc);
//             var material = physics->CreateMaterialMut(0.5f, 0.5f, 0.6f);

//             // create plane and add to scene
//             var plane = PxPlane_new_1(0.0f, 1.0f, 0.0f, 0.0f);
//             var groundPlane = physics->PhysPxCreatePlane(&plane, material);
//             scene->AddActorMut((PxActor*)groundPlane, null);

//             // create sphere and add to scene
//             var sphereGeo = PxSphereGeometry_new(10.0f);
//             var vec3 = new PxVec3 { x = 0.0f, y = 40.0f, z = 100.0f };
//             var transform = PxTransform_new_1(&vec3);
//             var identity = PxTransform_new_2(PxIDENTITY.PxIdentity);
//             var sphere = physics->PhysPxCreateDynamic(&transform, (PxGeometry*)&sphereGeo, material, 10.0f, &identity);
//             PxRigidBody_setAngularDamping_mut((PxRigidBody*)sphere, 0.5f);
//             scene->AddActorMut((PxActor*)sphere, null);

            
//         }
 
//         public void Update() {

//         }
//     }
// }
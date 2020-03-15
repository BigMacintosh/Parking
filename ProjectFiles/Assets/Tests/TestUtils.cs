using System;
using Game.Core.Parking;
using Game.Entity;
using NSubstitute;
using UnityEngine;
using Utils;

namespace Tests {
    public static class TestUtils {
        public static ParkingSpaceController NewMockParkingSpaceController(int n) {
            var transformController = Substitute.For<ISpaceTransformController>();
            transformController.GetTransform().Returns(new ObjectTransform {
                Position = new Vector3(n, n),
                Rotation = new Quaternion(n, n, n, n),
            });

            var colourController = Substitute.For<ISpaceColourController>();
            var parkingSpace     = Substitute.For<ParkingSpaceController>();
            parkingSpace.SpaceID             = (ushort) n;
            parkingSpace.TransformController = transformController;
            parkingSpace.ColourController    = colourController;
            parkingSpace.Disable();
            return parkingSpace;
        }

        public static Player NewMockPlayer(int n) {
            var player = Substitute.For<Player>(n, new PlayerOptions {
                CarColour  = Color.blue,
                CarType    = CarType.Hatchback,
                PlayerName = "",
            }, false);

            player.WhenForAnyArgs(x => x.Spawn((PlayerPosition) default))
                  .Do(x => Debug.Log($"Spawned Player {n}"));

            player.WhenForAnyArgs(x => x.Spawn((SpawnLocations) default))
                  .Do(x => Debug.Log($"Spawned Player {n}"));

            player.WhenForAnyArgs(x => x.Move(default))
                  .Do(x => throw new Exception("Should not be moving the player!"));

            player.When(x => x.DestroyCar())
                  .Do(x => Debug.Log($"Destroyed Player {n}"));

            player.GetPosition().Returns(new PlayerPosition());

            return player;
        }
    }
}
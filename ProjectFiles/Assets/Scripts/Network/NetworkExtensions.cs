using System;
using Game.Core.Driving;
using Game.Entity;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using Utils;

namespace Network {
    public static class NetworkExtensions {
        public static string IpAddress(this NetworkEndPoint endpoint) {
            /*
             * TODO: Find a better way of doing this. The NetworkEndPoint struct stores the socket data inside of an
             *  internal byte array, with the IP being stored at byte 4. This uses pointers to access bytes 4 - 8 and
             *  converts it into an IPv4 address.
             */
            unsafe {
                var rawBytes = (byte*) &endpoint + 4;
                return $"{rawBytes[0]}.{rawBytes[1]}.{rawBytes[2]}.{rawBytes[3]}";
            }
        }

        // turns out that structs are implicitly sealed in C# due to them having fixed size so we can't extend the
        // DataStreamWriter/Reader. Instead there's just lots of extension methods here for Unity types

        public static int WriterLength(this Vector3 vec) {
            return sizeof(float) * 3;
        }

        public static void WriteVector3(this DataStreamWriter writer, Vector3 vector) {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        public static Vector3 ReadVector3(this DataStreamReader reader, ref DataStreamReader.Context context) {
            var vector = new Vector3 {
                x = reader.ReadFloat(ref context),
                y = reader.ReadFloat(ref context),
                z = reader.ReadFloat(ref context),
            };
            return vector;
        }


        public static int WriterLength(this Quaternion quaternion) {
            return sizeof(float) * 4;
        }

        public static void WriteQuaternion(this DataStreamWriter writer, Quaternion quaternion) {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.w);
        }

        public static Quaternion ReadQuaternion(this DataStreamReader reader, ref DataStreamReader.Context context) {
            var quaternion = new Quaternion {
                x = reader.ReadFloat(ref context),
                y = reader.ReadFloat(ref context),
                z = reader.ReadFloat(ref context),
                w = reader.ReadFloat(ref context),
            };
            return quaternion;
        }

        public static int WriterLength(this PlayerPosition pos) {
            return pos.Transform.Position.WriterLength() + pos.Transform.Rotation.WriterLength() +
                   pos.Velocity.WriterLength()           + pos.AngularVelocity.WriterLength();
        }

        public static void WritePlayerPosition(this DataStreamWriter writer, PlayerPosition playerPosition) {
            writer.WriteVector3(playerPosition.Transform.Position);
            writer.WriteQuaternion(playerPosition.Transform.Rotation);
            writer.WriteVector3(playerPosition.Velocity);
            writer.WriteVector3(playerPosition.AngularVelocity);
        }

        public static PlayerPosition ReadPlayerPosition(this DataStreamReader         reader,
                                                        ref  DataStreamReader.Context context) {
            return new PlayerPosition {
                Transform = new ObjectTransform {
                    Position = reader.ReadVector3(ref context),
                    Rotation = reader.ReadQuaternion(ref context),
                },
                Velocity = reader.ReadVector3(ref context),
                AngularVelocity = reader.ReadVector3(ref context),
            };
        }

        public static int WriterLength(this Color col) {
            return sizeof(float) * 4;
        }

        public static void WriteColor(this DataStreamWriter writer, Color colour) {
            writer.Write(colour.r);
            writer.Write(colour.g);
            writer.Write(colour.b);
            writer.Write(colour.a);
        }

        public static Color ReadColor(this DataStreamReader reader, ref DataStreamReader.Context context) {
            return new Color {
                r = reader.ReadFloat(ref context),
                g = reader.ReadFloat(ref context),
                b = reader.ReadFloat(ref context),
                a = reader.ReadFloat(ref context),
            };
        }

        public static int WriterLength(this PlayerOptions playerOptions) {
            return playerOptions.CarColour.WriterLength() + sizeof(byte) + new NativeString64(playerOptions.PlayerName).LengthInBytes + 2;
        }

        public static void WritePlayerOptions(this DataStreamWriter writer, PlayerOptions playerOptions) {
            writer.WriteColor(playerOptions.CarColour);
            writer.Write((byte) playerOptions.CarType);
            writer.WriteString(playerOptions.PlayerName);
        }

        public static PlayerOptions ReadPlayerOptions(this DataStreamReader         reader,
                                                      ref  DataStreamReader.Context context) {
            return new PlayerOptions {
                CarColour  = reader.ReadColor(ref context),
                CarType    = (CarType) reader.ReadByte(ref context),
                PlayerName = reader.ReadString(ref context).ToString(),
            };
        }

        public static void WriteVehicleInputState(this DataStreamWriter writer, VehicleInputState inputs) {
            writer.Write(inputs.Drive);
            writer.Write(inputs.Turn);
            writer.Write(inputs.Jump);
            writer.Write(inputs.Drift);
        }

        public static VehicleInputState ReadVehicleInputState(this DataStreamReader reader, ref DataStreamReader.Context context) {
            return new VehicleInputState {
                Drive = reader.ReadFloat(ref context),
                Turn = reader.ReadFloat(ref context),
                Jump = reader.ReadFloat(ref context),
                Drift = reader.ReadFloat(ref context)
            };
        }

        public static int WriterLength(this VehicleInputState inputs) {
            return 4 * sizeof(float);
        }
    }
}
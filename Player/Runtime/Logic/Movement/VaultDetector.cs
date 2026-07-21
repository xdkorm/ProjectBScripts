using UnityEngine;
using ZigdarkS.ProjectB.Player.View;
using ZigdarkS.ProjectB.Player.Data;

namespace ZigdarkS.ProjectB.Player.Logic.Movement
{
    public enum VaultType { None, Vault, Mantle }

    public readonly struct VaultResult
    {
        public readonly VaultType Type;
        public readonly Vector3 LandingPosition; // позиция ног (низ капсулы) после перелезания

        public VaultResult(VaultType type, Vector3 landingPosition)
        {
            Type = type;
            LandingPosition = landingPosition;
        }

        public static VaultResult None => new VaultResult(VaultType.None, Vector3.zero);
    }

    public class VaultDetector
    {
        public VaultResult TryDetect(PlayerView view, PlayerConfig config)
        {
            var vaultConfig = config.Vault;
            float radius = view.ControllerRadius;
            Vector3 feetPos = view.Position;
            Vector3 forward = view.Forward;
            forward.y = 0f;
            forward.Normalize();

            // 1. Ищем стену впереди на уровне груди
            Vector3 chestOrigin = feetPos + Vector3.up * (radius + 0.1f);
            if (!Physics.SphereCast(chestOrigin, radius * 0.9f, forward, out RaycastHit wallHit, vaultConfig.ForwardCheckDistance))
            {
                return VaultResult.None;
            }

            // Стена должна быть примерно вертикальной (не пол/скат)
            if (Vector3.Angle(wallHit.normal, Vector3.up) < 60f)
            {
                return VaultResult.None;
            }

            // 2. Ищем верхнюю кромку препятствия — кастуем вниз с высокой точки над стеной
            Vector3 topCheckOrigin = feetPos + forward * (wallHit.distance + radius + 0.05f) + Vector3.up * vaultConfig.TopCheckHeight;
            if (!Physics.Raycast(topCheckOrigin, Vector3.down, out RaycastHit topHit, vaultConfig.TopCheckHeight))
            {
                return VaultResult.None; // нет потолка над препятствием — слишком высокое либо открытое пространство
            }

            float obstacleHeight = topHit.point.y - feetPos.y;

            VaultType type;
            if (obstacleHeight < vaultConfig.MinObstacleHeight)
            {
                return VaultResult.None; // слишком низкое — пусть игрок просто перешагнёт (step offset)
            }
            else if (obstacleHeight <= vaultConfig.MaxVaultHeight)
            {
                type = VaultType.Vault;
            }
            else if (obstacleHeight <= vaultConfig.MaxMantleHeight)
            {
                type = VaultType.Mantle;
            }
            else
            {
                return VaultResult.None; // слишком высокое
            }

            // 3. Проверяем, что за кромкой есть место приземлиться (капсула помещается)
            Vector3 landingPos = topHit.point + forward * vaultConfig.LandingForwardOffset;
            landingPos.y = topHit.point.y;

            Vector3 clearanceCheckOrigin = landingPos + Vector3.up * (config.Hitbox.StandingHeight / 2f + 0.05f);
            if (Physics.CheckSphere(clearanceCheckOrigin, radius * 0.9f))
            {
                return VaultResult.None; // место занято — нельзя приземлиться
            }

            return new VaultResult(type, landingPos);
        }
    }
}
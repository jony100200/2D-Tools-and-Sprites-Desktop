using UnityEngine;

namespace ABS
{
    public static class CameraHelper
    {
        public static void LocateMainCameraToModel(Model model, Studio studio, float turnAngle = 0f)
        {
            if (Camera.main == null || model == null)
                return;

            Vector3 modelToCamDir = new Vector3(0,
                Mathf.Sin(studio.view.slopeAngle * Mathf.Deg2Rad),
                Mathf.Cos(studio.view.slopeAngle * Mathf.Deg2Rad));

            float modelToCamDist = model.hasSpecificCameraValue ? model.cameraDistance : studio.cam.distance;

            Transform mainCamT = Camera.main.transform;
            Vector3 newCamPos = model.ComputedCenter + modelToCamDir * modelToCamDist;
            newCamPos.x += model.cameraOffset.x;
            newCamPos.y -= model.cameraOffset.y;
            newCamPos.z -= model.cameraOffset.z;
            Quaternion camRot = Quaternion.LookRotation(-modelToCamDir);
            mainCamT.SetPositionAndRotation(newCamPos, camRot);

            if (studio.view.rotationType == RotationType.Camera)
                mainCamT.RotateAround(model.GetPosition(), Vector3.down, turnAngle);

            Camera.main.farClipPlane = modelToCamDist * 2;

            if (studio.lit.com != null)
            {
                if (studio.lit.followCameraPosition)
                    studio.lit.com.transform.position = mainCamT.position;
                if (studio.lit.followCameraRotation)
                    studio.lit.com.transform.rotation = mainCamT.rotation;
            }
        }

        public static void RotateCameraToModel(Transform transf, Model model)
        {
            if (transf == null || model == null)
                return;

            Vector3 dirToModel = model.ComputedCenter - transf.position;
            Vector3 rightDir = Vector3.Cross(dirToModel, Vector3.up);
            transf.rotation = Quaternion.LookRotation(dirToModel.normalized, Vector3.Cross(dirToModel, rightDir));
        }

        public static float CalcFitOrthographicCameraSize(Model model)
        {
            MeshModel meshModel = Model.AsMeshModel(model);
            Bounds bounds = meshModel.GetBounds();

            var bndCornerPositions = new Vector3[]
            {
                new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)
            };

            var bndMinPosInScreen = new Vector2(float.MaxValue, float.MaxValue);
            var bndMaxPosInScreen = new Vector2(float.MinValue, float.MinValue);

            foreach (Vector3 worldPos in bndCornerPositions)
            {
                var screenPos = Camera.main.WorldToScreenPoint(worldPos);
                bndMinPosInScreen.x = Mathf.Min(bndMinPosInScreen.x, screenPos.x);
                bndMaxPosInScreen.x = Mathf.Max(bndMaxPosInScreen.x, screenPos.x);
                bndMinPosInScreen.y = Mathf.Min(bndMinPosInScreen.y, screenPos.y);
                bndMaxPosInScreen.y = Mathf.Max(bndMaxPosInScreen.y, screenPos.y);
            }

            float bndWidthInScreen = bndMaxPosInScreen.x - bndMinPosInScreen.x;
            float bndHeightInScreen = bndMaxPosInScreen.y - bndMinPosInScreen.y;

            float screenWidth = Camera.main.pixelWidth;
            float screenHeight = Camera.main.pixelHeight;

            float orthoSize = Camera.main.orthographicSize;

            if (bndWidthInScreen <= 0.0f && bndHeightInScreen <= 0.0f)
            {
                Debug.LogWarning("bndWidthInScreen <= 0.0f && bndHeightInScreen <= 0.0f");
                return orthoSize;
            }

            if (bndWidthInScreen > screenWidth || bndHeightInScreen > screenHeight)
            {
                float widthScaleRatio = bndWidthInScreen / screenWidth;
                float heightScaleRatio = bndHeightInScreen / screenHeight;
                orthoSize *= Mathf.Max(widthScaleRatio, heightScaleRatio);
            }
            else if (bndWidthInScreen < screenWidth && bndHeightInScreen < screenHeight)
            {
                float widthScaleRatio = screenWidth / bndWidthInScreen;
                float heightScaleRatio = screenHeight / bndHeightInScreen;
                orthoSize /= Mathf.Min(widthScaleRatio, heightScaleRatio);
            }

            return orthoSize;
        }
    }
}

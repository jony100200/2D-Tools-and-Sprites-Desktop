using UnityEngine;
using System.Collections.Generic;

namespace KalponicGames.KS_SnapStudio
{
    /// <summary>
    /// Common validation utilities for consistent error checking across components
    /// Follows Universal Unity Coding Guide - Validation and Error Handling
    /// </summary>
    public static class ValidationUtils
    {
        /// <summary>
        /// Result of a validation operation
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; private set; }
            public string ErrorMessage { get; private set; }
            public List<string> Warnings { get; private set; }

            public ValidationResult(bool isValid, string errorMessage = null, List<string> warnings = null)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
                Warnings = warnings ?? new List<string>();
            }

            public string GetErrorMessage()
            {
                return ErrorMessage ?? "Validation failed";
            }

            public string GetFullMessage()
            {
                string message = GetErrorMessage();
                if (Warnings.Count > 0)
                {
                    message += "\nWarnings:\n" + string.Join("\n", Warnings);
                }
                return message;
            }
        }

        /// <summary>
        /// Validates frame index parameters
        /// </summary>
        public static ValidationResult ValidateFrameIndex(int frameIndex, int maxFrames)
        {
            var warnings = new List<string>();

            if (maxFrames <= 0)
            {
                return new ValidationResult(false, "Max frames must be greater than 0");
            }

            if (frameIndex < 0)
            {
                return new ValidationResult(false, $"Frame index {frameIndex} cannot be negative");
            }

            if (frameIndex >= maxFrames)
            {
                return new ValidationResult(false, $"Frame index {frameIndex} exceeds maximum frames {maxFrames}");
            }

            return new ValidationResult(true, null, warnings);
        }

        /// <summary>
        /// Validates texture parameters for preview display
        /// </summary>
        public static ValidationResult ValidateTexture(Texture2D texture)
        {
            var warnings = new List<string>();

            if (texture == null)
            {
                return new ValidationResult(false, "Texture cannot be null");
            }

            if (texture.width <= 0 || texture.height <= 0)
            {
                return new ValidationResult(false, $"Invalid texture dimensions: {texture.width}x{texture.height}");
            }

            if (texture.width > 8192 || texture.height > 8192)
            {
                warnings.Add("Large texture dimensions may impact performance");
            }

            return new ValidationResult(true, null, warnings);
        }

        /// <summary>
        /// Validates rectangle parameters
        /// </summary>
        public static ValidationResult ValidateRect(Rect rect)
        {
            if (rect.width <= 0 || rect.height <= 0)
            {
                return new ValidationResult(false, $"Invalid rectangle dimensions: {rect.width}x{rect.height}");
            }

            return new ValidationResult(true);
        }

        /// <summary>
        /// Validates preview display parameters
        /// </summary>
        public static ValidationResult ValidatePreviewDisplay(int currentFrameIndex, int totalFrames, Texture2D texture, Rect displayRect)
        {
            var frameValidation = ValidateFrameIndex(currentFrameIndex, totalFrames);
            if (!frameValidation.IsValid)
            {
                return frameValidation;
            }

            var textureValidation = ValidateTexture(texture);
            if (!textureValidation.IsValid)
            {
                return textureValidation;
            }

            var rectValidation = ValidateRect(displayRect);
            if (!rectValidation.IsValid)
            {
                return rectValidation;
            }

            // Combine warnings from all validations
            var allWarnings = new List<string>();
            allWarnings.AddRange(frameValidation.Warnings);
            allWarnings.AddRange(textureValidation.Warnings);
            allWarnings.AddRange(rectValidation.Warnings);

            return new ValidationResult(true, null, allWarnings);
        }

        /// <summary>
        /// Validates animation parameters
        /// </summary>
        public static ValidationResult ValidateAnimation(string animationName, int frameCount, float duration)
        {
            var warnings = new List<string>();

            if (string.IsNullOrEmpty(animationName))
            {
                return new ValidationResult(false, "Animation name cannot be null or empty");
            }

            if (frameCount <= 0)
            {
                return new ValidationResult(false, "Frame count must be greater than 0");
            }

            if (duration <= 0)
            {
                return new ValidationResult(false, "Animation duration must be greater than 0");
            }

            if (frameCount > 1000)
            {
                warnings.Add("Large frame count may impact performance");
            }

            if (duration > 300f)
            {
                warnings.Add("Long animation duration may impact performance");
            }

            return new ValidationResult(true, null, warnings);
        }

        /// <summary>
        /// Validates zoom level parameters
        /// </summary>
        public static ValidationResult ValidateZoomLevel(float zoomLevel)
        {
            if (zoomLevel <= 0f)
            {
                return new ValidationResult(false, $"Zoom level {zoomLevel} must be greater than 0");
            }

            if (zoomLevel > 10f)
            {
                return new ValidationResult(false, $"Zoom level {zoomLevel} is too high (max: 10x)");
            }

            var warnings = new List<string>();
            if (zoomLevel > 3f)
            {
                warnings.Add("High zoom levels may impact performance");
            }

            return new ValidationResult(true, null, warnings);
        }
    }
}

using System;
using System.Collections.Generic;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapSphereObject : MapCircleObject
    {
        protected MapSphereObject(ObjectCreateParams creationParameters)
            : base(creationParameters)
        {
        }

        protected override List<(float centerX, float centerZ, float radius)> Get2DDimensions()
        {
            List<(float centerX, float centerY, float centerZ, float radius3D)> dimensions3D = Get3DDimensions();
            List<(float centerX, float centerZ, float radius)> dimensions2D = dimensions3D.ConvertAll(
                dimensions =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float yDiff = marioY - dimensions.centerY;
                    float radiusSquared = dimensions.radius3D * dimensions.radius3D - yDiff * yDiff;
                    float radius2D = radiusSquared >= 0 ? (float)Math.Sqrt(radiusSquared) : 0;
                    return (dimensions.centerX, dimensions.centerZ, radius2D);
                });
            return dimensions2D;
        }

        protected abstract List<(float centerX, float centerY, float centerZ, float radius3D)> Get3DDimensions();

        protected override void DrawOrthogonal(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var color = new Vector4(Color.R / 255.0f, Color.G / 255.0f, Color.B / 255.0f, (float)Opacity);
                foreach (var dim in Get3DDimensions())
                {
                    var transform = Matrix4.CreateScale(dim.radius3D) * Matrix4.CreateTranslation(dim.centerX, dim.centerY, dim.centerZ);
                    graphics.sphereRenderer.Add(transform, color);
                }
            });
        }
    }
}

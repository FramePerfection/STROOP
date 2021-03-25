﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Drawing.Imaging;

namespace STROOP.Map
{
    public abstract class MapIconObject : MapObject
    {
        protected Image Image;
        protected int TextureId;

        public MapIconObject()
            : base()
        {
            Image = null;
            TextureId = -1;
        }

        protected void UpdateImage()
        {
            var myImage = GetImage();
            Lazy<Image> image = myImage ?? Config.ObjectAssociations.EmptyImage;
            if (image.Value != Image)
            {
                Image = image.Value;
                GL.DeleteTexture(TextureId);
                TextureId = MapUtilities.LoadTexture(Image as Bitmap);
            }
        }

        public override MapDrawType GetDrawType()
        {
            return MapDrawType.Overlay;
        }

        public override void Update()
        {
            UpdateImage();
        }
    }
}

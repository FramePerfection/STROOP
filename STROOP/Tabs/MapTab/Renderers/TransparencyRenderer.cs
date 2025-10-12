using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;
using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.Renderers
{
    public class TransparencyRenderer : InstanceRenderer<SpriteRenderer.InstanceData>
    {
        public interface Transparent
        {
            void Prepare(TransparencyRenderer renderer);
            void DrawMask(TransparencyRenderer renderer);
            void DrawTransparent(TransparencyRenderer renderer);
        }

        int[] maskTexture;
        int colorBuffer;
        int maskFBO, renderFBO;
        int stencilBuffer;
        int renderTexture;
        int width, height;

        Func<int> originalDepthGetter;

        bool canStencil;

        public float solidDepthBias = 0.01f;
        public List<Transparent> transparents = new List<Transparent>();
        public MapGraphics graphics { get; private set; }

        public TransparencyRenderer(int maxDepthComplexity, Func<int> originalDepthGetter, int initialWidth, int initialHeight)
        {
            this.originalDepthGetter = originalDepthGetter;
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                shader = GraphicsUtil.GetShaderProgram("Resources/Shaders/Sprites.vert.glsl", "Resources/Shaders/BlendTransparency.frag.glsl");
                Init(1);
                SpriteRenderer.GenSpriteVAO(vertexArray, instanceBuffer);
                instances.Add(new SpriteRenderer.InstanceData() { transform = Matrix4.CreateScale(2), textureIndex = 0 });
                UpdateBuffer(instances.Count);

                maskFBO = GL.GenFramebuffer();
                colorBuffer = GL.GenTexture();

                canStencil = true;
            //Generate the Framebuffer for creating the masks front to back
            tryMakeMaskFBO:
                maskFBO = GL.GenFramebuffer();
                stencilBuffer = GL.GenRenderbuffer();
                renderTexture = GL.GenTexture();
                renderFBO = GL.GenFramebuffer();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderFBO);
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderTexture, 0);
                GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                SetMaxDepthComplexity(maxDepthComplexity);

                SetDimensions(initialWidth, initialHeight);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, maskFBO);
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, maskTexture[0], 0);
                if (canStencil)
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, stencilBuffer);
                GL.DrawBuffer(DrawBufferMode.None);

                FramebufferErrorCode error;
                if ((error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)) != FramebufferErrorCode.FramebufferComplete)
                {
                    GL.DeleteFramebuffer(maskFBO);
                    if (!canStencil)
                        throw new NotSupportedException("Transparency rendering doesn't work on this system.");
                    canStencil = false;
                    goto tryMakeMaskFBO;
                }

                if ((error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)) != FramebufferErrorCode.FramebufferComplete)
                {
                    throw null;
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            });
        }

        public void SetMaxDepthComplexity(int complexity)
        {
            if (maskTexture != null)
                GL.DeleteTextures(maskTexture.Length, maskTexture);
            maskTexture = new int[complexity];
            GL.GenTextures(complexity, maskTexture);
            for (int i = 0; i < complexity; i++)
            {
                GL.BindTexture(TextureTarget.Texture2D, maskTexture[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }
        }

        public void SetDimensions(int width, int height)
        {
            this.width = width;
            this.height = height;
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, stencilBuffer);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.StencilIndex1, width, height);

                GL.BindTexture(TextureTarget.Texture2D, renderTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.HalfFloat, IntPtr.Zero);

                for (int i = 0; i < maskTexture.Length; i++)
                {
                    GL.BindTexture(TextureTarget.Texture2D, maskTexture[i]);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                }
            });
        }

        public void CleanUp()
        {
            GL.DeleteTextures(maskTexture.Length, maskTexture);
            GL.DeleteRenderbuffer(stencilBuffer);
            GL.DeleteTexture(colorBuffer);
            GL.DeleteTexture(renderTexture);
            GL.DeleteFramebuffer(maskFBO);
            GL.DeleteFramebuffer(renderFBO);
        }

        int currentMaskLayer = -1;
        int solidDepthBuffer;
        public int GetReferenceDepthBuffer() => currentMaskLayer == 0 ? 0 : maskTexture[currentMaskLayer - 1];

        public void SetUniforms(int shader)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, GetReferenceDepthBuffer());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, solidDepthBuffer);
            GL.Uniform1(GL.GetUniformLocation(shader, "referenceDepthMask"), 0);
            GL.Uniform1(GL.GetUniformLocation(shader, "referenceSolidDepth"), 1);
            GL.Uniform1(GL.GetUniformLocation(shader, "readDepthMask"), (int)currentMaskLayer);
            GL.Uniform1(GL.GetUniformLocation(shader, "solidDepthBias"), 0.05f);
            GL.Uniform1(GL.GetUniformLocation(shader, "zNear"), graphics.nearClip);
            GL.Uniform1(GL.GetUniformLocation(shader, "zFar"), graphics.farClip);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public override void SetDrawCalls(MapGraphics graphics)
        {
            if (graphics.view.mode != MapView.ViewMode.TopDown)
                graphics.drawLayers[(int)MapGraphics.DrawLayers.Transparency].Add(() =>
                {
                    var error = GL.GetError();
                    Render(graphics, originalDepthGetter());
                });
        }

        public void Render(MapGraphics graphics, int worldDepthBuffer)
        {
            this.graphics = graphics;
            this.solidDepthBuffer = worldDepthBuffer;

            foreach (var t in transparents)
                t.Prepare(this);

            int[] prevViewport = new int[4];
            GL.GetInteger(GetPName.Viewport, prevViewport);
            GL.GetInteger(GetPName.FramebufferBinding, out int prevFramebuffer);
            GL.Viewport(0, 0, width, height);

            GL.ClearDepth(1);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, maskFBO);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            for (currentMaskLayer = 0; currentMaskLayer < maskTexture.Length; currentMaskLayer++)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, maskTexture[currentMaskLayer], 0);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                foreach (var t in transparents)
                    t.DrawMask(this);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderFBO);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderTexture, 0);
            GL.ClearColor(0, 0.0f, 0, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.DepthFunc(DepthFunction.Equal);
            GL.Disable(EnableCap.StencilTest);

            for (int i = currentMaskLayer - 1; i >= 0; i--)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, maskTexture[i], 0);
                foreach (var t in transparents)
                    t.DrawTransparent(this);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFramebuffer);
            GL.Viewport(prevViewport[0], prevViewport[1], prevViewport[2], prevViewport[3]);

            ignoreView = true;
            BeginDraw(graphics);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, renderTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.Uniform1(GL.GetUniformLocation(shader, "sampler"), (int)0);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            GL.DrawArraysInstanced(PrimitiveType.TriangleStrip, 0, 4, instances.Count);
            GL.BindVertexArray(0);


            solidDepthBuffer = 0;
            graphics = null;
        }
    }
}

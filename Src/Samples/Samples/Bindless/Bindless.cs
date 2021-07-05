﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vultaik.Desktop;
using Vultaik;
using Vultaik.Physics;
using Buffer = Vultaik.Buffer;
using Vortice.Vulkan;
using Interop = Vultaik.Interop;
using Samples.Common;
using Vultaik.Toolkit;

namespace Samples.Bindless
{
    public class Bindless : ExampleBase, IDisposable
    {
        private const int TextureWidth = 256; //Texture Data
        private const int TextureHeight = 256; //Texture Data
        private const int TexturePixelSize = 4;  // The number of bytes used to represent a pixel in the texture. RGBA


        private AdapterConfig AdapterConfig;
        private Adapter Adapter;
        private Device Device;
        private Framebuffer Framebuffer;
        private SwapChain SwapChain;
        private GraphicsContext Context;
        private GraphicsPipeline PipelineState_0;
        private DescriptorSet DescriptorSet_0;
        private Dictionary<string, Buffer> Buffers = new();
        private TransformUniform uniform;



        public Bindless() : base()
        {

        }


        public override void Initialize()
        {

            AdapterConfig = new()
            {
                SwapChain = true,
                VulkanDebug = false,
                Fullscreen = false,
                Bindless = true,
            };



            Adapter = new(AdapterConfig);
            Device = new(Adapter);
            SwapChain = new(Device, new()
            {
                Source = GetSwapchainSource(Adapter),
                ColorSrgb = false,
                Height = Window.Height,
                Width = Window.Width,
                VSync = false,
                DepthFormat = Adapter.DepthFormat is VkFormat.Undefined ? null : Adapter.DepthFormat
            });

            Context = new(Device);
            Framebuffer = new(SwapChain);


            Camera.SetPosition(0, 0.34f, -18.5f);
            uniform = new(Camera.Projection, Camera.View);


            CreateBuffers();
            CreatePipelineState();
        }

        int[] Indices = new[]
        {
               0,1,2, 0,2,3, 4,5,6,  4,6,7, 8,9,10, 8,10,11, 12,13,14, 12,14,15, 16,17,18, 16,18,19, 20,21,22, 20,22,23
        };

        VertexPositionTexture[] Vertices = new VertexPositionTexture[]
        {
                new(new(-0.5f, -0.5f,  0.5f), new(0.0f, 0.0f)),
                new(new(0.5f, -0.5f,  0.5f), new(1.0f, 0.0f)),
                new(new(0.5f,  0.5f,  0.5f),  new(1.0f, 1.0f)),
                new(new(-0.5f, 0.5f,  0.5f ),  new(0.0f, 1.0f)),

                new(new(0.5f,  0.5f,  0.5f), new(0.0f, 0.0f)),
                new(new(0.5f,  0.5f, -0.5f), new(1.0f, 0.0f)),
                new(new(0.5f, -0.5f, -0.5f),  new(1.0f, 1.0f)),
                new(new(0.5f, -0.5f, -0.5f ),  new(0.0f, 1.0f)),

                new(new(0.5f, -0.5f, -0.5f ), new(0.0f, 0.0f)),
                new(new( 0.5f, -0.5f, -0.5f ), new(1.0f, 0.0f)),
                new(new( 0.5f,  0.5f, -0.5f),  new(1.0f, 1.0f)),
                new(new(-1.0f,  1.0f, -1.0f),  new(0.0f, 1.0f)),


                new(new(-0.5f, -0.5f, -0.5f ), new(0.0f, 0.0f)),
                new(new(-0.5f, -0.5f,  0.5f ), new(1.0f, 0.0f)),
                new(new( -0.5f,  0.5f,  0.5f ),  new(1.0f, 1.0f)),
                new(new(-0.5f,  0.5f, -0.5f ),  new(0.0f, 1.0f)),

                new(new( 0.5f,  0.5f,  0.5f), new(0.0f, 0.0f)),
                new(new(-0.5f,  0.5f,  0.5f ), new(1.0f, 0.0f)),
                new(new( -0.5f,  0.5f, -0.5f  ),  new(1.0f, 1.0f)),
                new(new( 1.0f,  1.0f, -1.0f ),  new(0.0f, 1.0f)),

                new(new( -0.5f, -0.5f, -0.5f), new(0.0f, 0.0f)),
                new(new(0.5f, 0.5f, -0.5f), new(1.0f, 0.0f)),
                new(new(0.5f, -0.5f,  0.5f ),  new(1.0f, 1.0f)),
                new(new( -0.5f, -0.5f,  0.5f ),  new(0.0f, 1.0f)),
        };


        public void CreateBuffers()
        {




            Buffers["VertexBuffer"] = new(Device, new()
            {
                BufferFlags = BufferFlags.VertexBuffer,
                Usage = ResourceUsage.CPU_To_GPU,
                SizeInBytes = Interop.SizeOf(Vertices),
            });
            Buffers["VertexBuffer"].SetData(Vertices);


            Buffers["IndexBuffer"] = new(Device, new()
            {
                BufferFlags = BufferFlags.IndexBuffer,
                Usage = ResourceUsage.CPU_To_GPU,
                SizeInBytes = Interop.SizeOf(Indices),
            });
            Buffers["IndexBuffer"].SetData(Indices);


            Buffers["ConstBuffer1"] = new(Device, new()
            {
                BufferFlags = BufferFlags.ConstantBuffer,
                Usage = ResourceUsage.CPU_To_GPU,
                SizeInBytes = Interop.SizeOf<TransformUniform>(),
            });

            Buffers["ConstBuffer2"] = new(Device, new()
            {
                BufferFlags = BufferFlags.ConstantBuffer,
                Usage = ResourceUsage.CPU_To_GPU,
                SizeInBytes = Interop.SizeOf<TransformUniform>(),
            });






            // Prepare per-object matrices with random rotations
            Random rndGen = new Random();
            Func<Random, float> rndDist = rand => (float)(rand.NextDouble() * 2 - 1.0);

            for (uint i = 0; i < OBJECT_INSTANCES; i++)
            {
                random_texture[i] = random.Next(0, 8);
                rotationSpeeds[i] = new Vector3(rndDist(rndGen), rndDist(rndGen), rndDist(rndGen));

            }
        }
        Random random = new();
        private Vector3[] rotationSpeeds = new Vector3[OBJECT_INSTANCES]; // Store random per-object rotations

        int[] random_texture = new int[OBJECT_INSTANCES]; // random.Next(0, 3);
        private const uint OBJECT_INSTANCES = 75;

        public void AddCube(CommandBuffer cmd, Vector3 position, Vector3 rotation, bool r, int text)
        {

            Matrix4x4 model = Matrix4x4.Identity;


            model = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) * Matrix4x4.CreateTranslation(position) * Matrix4x4.CreateScale(0.8f);


            cmd.PushConstant(PipelineState_0, ShaderStage.Vertex, model);
            cmd.PushConstant<int>(PipelineState_0, ShaderStage.Fragment, text);
            cmd.BindDescriptorSets(DescriptorSet_0);
            cmd.DrawIndexed(Indices.Length, 1, 0, 0, 0);
            //cmd.DrawIndexed(6, 1, 0, 0, 0);

        }


        public void GenerateCubes(CommandBuffer cmd, bool r)
        {


            float rotation = Time.TotalMilliseconds / 600;

            uint dim = (uint)(Math.Pow(OBJECT_INSTANCES, (1.0f / 3.0f)));
            Vector3 offset = new Vector3(5.0f);

            for (uint x = 0; x < dim; x++)
            {
                for (uint y = 0; y < dim; y++)
                {
                    for (uint z = 0; z < dim; z++)
                    {
                        uint index = x * dim * dim + y * dim + z;

                        Vector3 rotations = rotation * rotationSpeeds[index];
                        int r_t = random_texture[index];
                        Vector3 pos = new Vector3(-((dim * offset.X) / 2.0f) + offset.X / 2.0f + x * offset.X, -((dim * offset.Y) / 2.0f) + offset.Y / 2.0f + y * offset.Y, -((dim * offset.Z) / 2.0f) + offset.Z / 2.0f + z * offset.Z);
                        Console.WriteLine(r_t);
                        AddCube(cmd, pos, rotations, r, r_t);
                    }
                }
            }
        }


        public void CreatePipelineState()
        {


            string shaders = Constants.ShadersFile;
            string images = Constants.ImagesFile;


            string Fragment = shaders + "Bindless/Fragment.hlsl";
            string Vertex = shaders + "Bindless/Vertex.hlsl";



            //var img0 = Image(GenerateTextureData(), TextureWidth, TextureWidth, 1, 1, TextureWidth * TextureWidth * 4, false, vkf.R8G8B8A8UNorm);
            Image text1 = ImageFile.Load2DFromFile(Device, images + "IndustryForgedDark512.ktx");
            Image text2 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap09-512.png");
            Image text3 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap05-512.png");
            Image text4 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap13-512.png");
            Image text5 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap10-512.png");
            Image text6 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap12-1024.png");
            Image text7 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap08-512.png");
            Image text8 = ImageFile.Load2DFromFile(Device, images + "UVCheckerMap11-512.png");
            Sampler sampler = new Sampler(Device);


            GraphicsPipelineDescription Pipelinedescription_0 = new();
            Pipelinedescription_0.SetFramebuffer(Framebuffer);
            Pipelinedescription_0.SetShader(new ShaderBytecode(Fragment, ShaderStage.Fragment));
            Pipelinedescription_0.SetShader(new ShaderBytecode(Vertex, ShaderStage.Vertex));
            Pipelinedescription_0.SetVertexBinding(VkVertexInputRate.Vertex, VertexPositionTexture.Size);
            Pipelinedescription_0.SetVertexAttribute(VertexType.Position);
            Pipelinedescription_0.SetVertexAttribute(VertexType.TextureCoordinate);
            //Pipelinedescription_0.SetCullMode(VkCullModeFlags.None);
            PipelineState_0 = new(Pipelinedescription_0);

            DescriptorData descriptorData_0 = new();
            descriptorData_0.SetUniformBuffer(0, Buffers["ConstBuffer1"]);
            descriptorData_0.SetBindlessImage(1, new[] { text1, text2, text3, text4, text5, text6, text7, text7 });
            descriptorData_0.SetSampler(2, sampler);
            DescriptorSet_0 = new(PipelineState_0, descriptorData_0);

        }



        public override void Update(ApplicationTime time)
        {
            Camera.Update();
            uniform.Update(Camera);
            Buffers["ConstBuffer1"].SetData(ref uniform);


            Rotation.Z += 0.010f * MathF.PI;

        }



        public override void Draw(ApplicationTime time)

        {

            Device.WaitIdle();
            CommandBuffer commandBuffer = Context.CommandBuffer;
            commandBuffer.Begin();


            commandBuffer.BeginFramebuffer(Framebuffer);
            commandBuffer.SetScissor(Window.FramebufferSize.Width, Window.FramebufferSize.Height, 0, 0);
            commandBuffer.SetViewport(Window.FramebufferSize.Width, Window.FramebufferSize.Height, 0, 0);





            commandBuffer.SetVertexBuffers(new Buffer[] { Buffers["VertexBuffer"] });
            commandBuffer.SetIndexBuffer(Buffers["IndexBuffer"]);


            commandBuffer.SetGraphicPipeline(PipelineState_0);
            //commandBuffer.PushConstant(PipelineState_0, ShaderStage.Vertex, Model);
            //commandBuffer.PushConstant<int>(PipelineState_0, ShaderStage.Fragment, 1);

            GenerateCubes(commandBuffer, true);





            commandBuffer.Close();
            Device.Submit(commandBuffer);
            SwapChain.Present();
        }

        Random Random = new();
        public override void Resize(int width, int height)
        {
            Device.WaitIdle();
            SwapChain.Resize(width, height);
            Framebuffer.Resize();

            Camera.AspectRatio = (float)width / height;
        }


        internal byte[] GenerateTextureData()
        {
            byte r = 255;
            byte g = 255;
            byte b = 255;
            byte a = 255;

            int color = default;
            color |= r << 24;
            color |= g << 16;
            color |= b << 8;
            color |= a;

            uint color_value = (uint)color; // RBGA

            byte color_r = (byte)((color_value >> 24) & 0xFF);
            byte color_g = (byte)((color_value >> 16) & 0xFF);
            byte color_b = (byte)((color_value >> 8) & 0xFF);
            byte color_a = (byte)(color_value & 0xFF);


            int row_pitch = TextureWidth * TexturePixelSize;
            int cell_pitch = row_pitch >> 3;       // The width of a cell in the checkboard texture.
            int cell_height = TextureWidth >> 3;  // The height of a cell in the checkerboard texture.
            int texture_size = row_pitch * TextureHeight; // w * h * rgba = 4
            byte[] data = new byte[texture_size];

            for (int n = 0; n < texture_size; n += TexturePixelSize)
            {
                int x = n % row_pitch;
                int y = n / row_pitch;
                int i = x / cell_pitch;
                int j = y / cell_height;

                if (i % 2 == j % 2)
                {
                    data[n + 0] = 1; // R
                    data[n + 1] = 1; // G
                    data[n + 2] = 1; // B
                    data[n + 3] = 1; // A
                }
                else
                {
                    data[n + 0] = 0xff; // R
                    data[n + 1] = 0xff; // G
                    data[n + 2] = 0xff; // B
                    data[n + 3] = 0xff; // A
                }
            }

            return data;
        }

        public void Dispose()
        {
            Adapter.Dispose();
        }


    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TransformUniform
    {
        public TransformUniform(Matrix4x4 p,Matrix4x4 v)
        {
            P = p;
            V = v;
        }

        public Matrix4x4 V;
        public Matrix4x4 P;



        public void Update(Camera camera)
        {
            P = camera.Projection;
            V = camera.View;
        }
    }
}
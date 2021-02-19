﻿// Copyright (c) 2019-2020 Faber Leonardo. All Rights Reserved. https://github.com/FaberSanZ
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

/*=============================================================================
	PipelineStateDescription.cs
=============================================================================*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vortice.Vulkan;
using Zeckoxe.Core;

namespace Zeckoxe.Vulkan
{
    public static class VertexElementExt
    {
        public static int Size(this VertexType element)
        {
            switch (element)
            {
                case VertexType.Position: return 12;

                case VertexType.Normal: return 12;

                case VertexType.TextureCoordinate: return 8;

                case VertexType.Color :return 12;

                default: return 0;
            }
        }


        public static PixelFormat ToPixelFormat(this VertexType element)
        {
            switch (element)
            {
                case VertexType.Position: return PixelFormat.R32G32B32SFloat;

                case VertexType.Normal: return PixelFormat.R32G32B32SFloat;

                case VertexType.TextureCoordinate: return PixelFormat.R32G32SFloat;

                case VertexType.Color: return PixelFormat.R32G32B32SFloat;

                default: return 0;
            }
        }


        
    }


    public class PipelineStateDescription
    {
        internal List<ResourceInfo> resourceInfos = new();
        internal int VertexAttributeLocation = 0;
        internal int VertexAttributeOffset = 0;

        public PipelineStateDescription()
        {
            SetPrimitiveType(PrimitiveType.TriangleList);
            SetFillMode(FillMode.Solid);
            SetCullMode(CullMode.Front);
        }


        public Framebuffer Framebuffer { get; set; }

        public InputAssemblyState InputAssemblyState { get; set; } = new();

        public RasterizationState RasterizationState { get; set; } = new();

        public MultisampleState MultisampleState { get; set; } = new();

        public PipelineVertexInput PipelineVertexInput { get; set; } = new();

        public List<ShaderBytecode> Shaders { get; set; } = new();

        public List<DescriptorSetLayout> Layouts { get; set; } = new();

        public List<PushConstantRange> PushConstants { get; set; } = new();


        public void AddVertexAttribute<TVertex>()
        {
            IEnumerable<PropertyInfo> propertyInfos = typeof(TVertex).GetTypeInfo().GetRuntimeProperties();

            foreach(PropertyInfo info in propertyInfos)
            { 
                VertexAttribute attribute = info.GetCustomAttribute<VertexAttribute>();

                if (attribute.Type is VertexType.Position)
                    AddVertexAttribute(VertexType.Position);


                if (attribute.Type is VertexType.Color)
                    AddVertexAttribute(VertexType.Color);


                if (attribute.Type is VertexType.TextureCoordinate)
                    AddVertexAttribute(VertexType.TextureCoordinate);
            }
        }

        public void SetFramebuffer(Framebuffer framebuffer)
        {
            Framebuffer = framebuffer;
        }

        public void SetCullMode(CullMode mode)
        {
            RasterizationState.CullMode = mode;
        }

        public void SetFillMode(FillMode mode)
        {
            RasterizationState.FillMode = mode;
        }

        public void SetPrimitiveType(PrimitiveType type)
        {
            InputAssemblyState.PrimitiveType = type;
        }

        public void AddShader(ShaderBytecode bytecode)
        {
            if (bytecode.Data.Any())
            {
                Shaders.Add(bytecode);
            }
        }


        public void AddVertexBinding(VertexInputRate rate, int stride, int binding = 0)
        {
            PipelineVertexInput.VertexBindingDescriptions.Add(new()
            {
                Binding = binding,
                InputRate = rate,
                Stride = stride,
            });
        }
        public void AddVertexAttribute(VertexType element, int binding = 0)
        {
            PipelineVertexInput.VertexAttributeDescriptions.Add(new()
            {
                Binding = binding,
                Location = VertexAttributeLocation,
                Format = element.ToPixelFormat(),
                Offset = VertexAttributeOffset,
            });

            VertexAttributeLocation++;
            VertexAttributeOffset += element.Size();
        }



        public void SetImageSampler(int offset, ShaderStage stage, Image texture, Sampler sampler)
        {

            resourceInfos.Add(new ResourceInfo
            {
                _offset = offset,
                _binding = offset,
                is_sampler = true,
                is_texture = true,
                _sampler = sampler,
                _texture = texture,
                descriptor_type = Vortice.Vulkan.VkDescriptorType.CombinedImageSampler,
                shader_descriptor_type = stage.StageToVkShaderStageFlags()
            });
        }


        public void SetUniformBuffer(int binding, ShaderStage stage, Buffer buffer, int offset = 0)
        {
            resourceInfos.Add(new ResourceInfo
            {
                _offset = offset,
                _binding = binding,
                is_buffer = true,
                _buffer = buffer,
                descriptor_type = Vortice.Vulkan.VkDescriptorType.UniformBuffer,
                shader_descriptor_type = stage.StageToVkShaderStageFlags()
            });

        }

    }
}
﻿using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class UpdateProductAttributeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

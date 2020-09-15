using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO.CarDTO
{
    public class CarsImportDTO : Car
    {
        public int[] partsId { get; set; }
    }
}

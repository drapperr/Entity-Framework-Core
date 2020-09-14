using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using CarDealer.DTO.CarDTO;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<CarsImportDTO, Car>();

        }
    }
}

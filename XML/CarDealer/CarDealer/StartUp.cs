using CarDealer.Data;
using CarDealer.Dto.Export;
using CarDealer.Dto.Imort;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XmlFacade;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            var suppliersXml = File.ReadAllText("../../../Datasets/suppliers.xml");
            var partsXml = File.ReadAllText("../../../Datasets/parts.xml");
            var carsXml = File.ReadAllText("../../../Datasets/cars.xml");
            var customersXml = File.ReadAllText("../../../Datasets/customers.xml");
            var salesXml = File.ReadAllText("../../../Datasets/sales.xml");

            //ResetDatabase(context);

            //Import
            //Console.WriteLine(ImportSuppliers(context,suppliersXml));
            //Console.WriteLine(ImportParts(context, partsXml));
            //Console.WriteLine(ImportCars(context, carsXml));
            //Console.WriteLine(ImportCustomers(context, customersXml));
            //Console.WriteLine(ImportSales(context, salesXml));

            //Export
            //Console.WriteLine(GetCarsWithDistance(context));
            //Console.WriteLine(GetCarsFromMakeBmw(context));
            //Console.WriteLine(GetLocalSuppliers(context));
            //Console.WriteLine(GetCarsWithTheirListOfParts(context));
            //Console.WriteLine(GetTotalSalesByCustomer(context));
            Console.WriteLine(GetSalesWithAppliedDiscount(context));

        }

        private static void ResetDatabase(CarDealerContext context)
        {
            context.Database.EnsureDeleted();
            Console.WriteLine("Database was deleted!!!");
            context.Database.EnsureCreated();
            Console.WriteLine("Database was created!");
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var suppliersResult = XMLConverter.Deserializer<ImportSupplierDto>(inputXml, "Suppliers");

            var suppliers = suppliersResult
                .Select(s => new Supplier()
                {
                    Name = s.Name,
                    IsImporter = s.IsImporter
                })
                .ToArray();

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var partsResult = XMLConverter.Deserializer<ImportCarPartDto>(inputXml, "Parts");

            var supplierIds = context.Suppliers.Select(s => s.Id);

            var parts = partsResult
                .Where(p => supplierIds.Contains(p.SupplierId))
                .Select(p => new Part()
                {
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    SupplierId = p.SupplierId
                })
                .ToArray();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Length}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var carsDtos = XMLConverter.Deserializer<ImportCarDto>(inputXml, "Cars");

            var cars = new List<Car>();

            foreach (var carDto in carsDtos)
            {
                var uniqueParts = carDto.Parts.Select(s => s.Id).Distinct().ToArray();
                var realParts = uniqueParts.Where(id => context.Parts.Any(i => i.Id == id));

                var car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TraveledDistance,
                    PartCars = realParts.Select(id => new PartCar()
                    {
                        PartId = id
                    })
                    .ToArray()
                };

                cars.Add(car);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            var customersResult = XMLConverter.Deserializer<ImportCustomerDto>(inputXml, "Customers");

            var customers = customersResult
                .Select(c => new Customer()
                {
                    Name = c.name,
                    BirthDate = c.BirthDate,
                    IsYoungDriver = c.IsYanoungDriver
                })
                .ToArray();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var salesResult = XMLConverter.Deserializer<ImportSalesDto>(inputXml, "Sales");

            var sales = salesResult
                .Where(s => context.Cars.Select(x => x.Id).Contains(s.CarId))
                .Select(c => new Sale()
                {
                    CarId = c.CarId,
                    CustomerId = c.CustomerId,
                    Discount = c.Discount
                })
                .ToArray();

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Length}";
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.TravelledDistance > 2000000)
                .OrderBy(c => c.Make).ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new ExportCarDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToArray();

            var result = XMLConverter.Serialize<ExportCarDto>(cars, "cars");

            return result;
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var BMWCars = context.Cars
                .Where(c => c.Make == "BMW")
                .OrderBy(c=>c.Model)
                .ThenByDescending(x=>x.TravelledDistance)
                .Select(c => new ExportBMWCarsDto()
                {
                    Id= c.Id,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .ToArray();

            var result = XMLConverter.Serialize<ExportBMWCarsDto>(BMWCars, "cars");

            return result;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var localSuppliers = context.Suppliers
                .Where(s=>s.IsImporter==false)
                .Select(s=>new ExportSupplierDto() 
                {
                    Id= s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count()
                })
                .ToArray();

            var result = XMLConverter.Serialize<ExportSupplierDto>(localSuppliers, "suppliers");

            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .OrderByDescending(c=>c.TravelledDistance)
                .ThenBy(c=>c.Model)
                .Take(5)
                .Select(c=> new ExportCarWithPartsDto()
                {
                    Make= c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance,
                    Parts = c.PartCars.Select(p=> new ExportPartDto() 
                    { 
                        Name = p.Part.Name,
                        Price = p.Part.Price
                    })
                    .OrderByDescending(p=>p.Price)
                    .ToList()
                })
                .ToArray();

            var result = XMLConverter.Serialize<ExportCarWithPartsDto>(cars, "cars");

            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Sales
                .Select(x => new ExportCustomerDto()
                {
                    FullName = x.Customer.Name,
                    BoughtCars = 1,
                    SpentMoney = x.Car.PartCars.Sum(p=>p.Part.Price)
                })
                .OrderByDescending(x=>x.SpentMoney)
                .ToArray();

            var result = XMLConverter.Serialize<ExportCustomerDto>(customers, "customers");

            return result;
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {

            var sales = context.Sales
                .Select(s=> new ExportSaleDto()
                {
                    Car = new ExportCarAttDto()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartCars.Sum(p=>p.Part.Price),
                    PriceWithDiscount = s.Car.PartCars.Sum(p => p.Part.Price) - (s.Car.PartCars.Sum(p => p.Part.Price) * s.Discount/100)
                })
                .ToArray();

            var result = XMLConverter.Serialize<ExportSaleDto>(sales, "sales");

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TaxiService
{
    public class Location
    {
        public int X;
        public int Y;
        public Location(int x, int y) { X = x; Y = y; }
    }

    public class Driver
    {
        public int Id;
        public Location Position;
        public Driver(int id, int x, int y)
        {
            Id = id;
            Position = new Location(x, y);
        }
    }

    // Поиск по евклидову расстоянию
    public class SimpleSearch
    {
        public List<Driver> FindDrivers(Location order, List<Driver> drivers, int width, int height)
        {
            var driversWithDistances = drivers.Select(driver => 
            {
                int dx = order.X - driver.Position.X;
                int dy = order.Y - driver.Position.Y;
                return (driver, distance: Math.Sqrt(dx * dx + dy * dy));
            }).ToList();

            // сортируем по расстоянию и берем первых 5
            return driversWithDistances
                .OrderBy(x => x.distance)
                .Take(5)
                .Select(x => x.driver)
                .ToList();
        }
    }
    // Поиск по "манхэттенскому" расстоянию
    public class CitySearch
    {
        public List<Driver> FindDrivers(Location order, List<Driver> drivers, int width, int height)
        {
            return drivers
                .OrderBy(driver => 
                    Math.Abs(order.X - driver.Position.X) + Math.Abs(order.Y - driver.Position.Y))
                .Take(5)
                .ToList();
        }
    }

    // Поиск в расширяющемся квадрате
    public class SquareSearch
    {
        public List<Driver> FindDrivers(Location order, List<Driver> drivers, int width, int height)
        {
            List<Driver> found = new List<Driver>();
            HashSet<Driver> addedDrivers = new HashSet<Driver>();
            int currentRange = 0;
            int maxRange = Math.Max(width, height);

            while (found.Count < 5 && currentRange <= maxRange)
            {
                foreach (Driver driver in drivers)
                {
                    if (addedDrivers.Contains(driver)) continue; // пропускаем уже добавленных водителей
                    
                    int distanceX = Math.Abs(order.X - driver.Position.X);
                    int distanceY = Math.Abs(order.Y - driver.Position.Y);
                    int distance = Math.Max(distanceX, distanceY);
                    
                    if (distance == currentRange)
                    {
                        found.Add(driver);
                        addedDrivers.Add(driver);
                        if (found.Count >= 5) return found;
                    }
                }
                currentRange++;
            }
            return found;
        }
    }

    // класс бенчмарка 
    [MemoryDiagnoser]
    [RankColumn]
    public class TaxiBenchmarks
    {
        private List<Driver> _drivers = null!;
        private Location _orderLocation = null!;
                
        // настройка карты
        private const int MapWidth = 100;
        private const int MapHeight = 100;

        // тестируем разное количество водителей
        [Params(100, 500)] 
        public int DriversCount;

        private readonly SimpleSearch _simpleSearch = new SimpleSearch();
        private readonly CitySearch _citySearch = new CitySearch();
        private readonly SquareSearch _squareSearch = new SquareSearch();

        [GlobalSetup]
        public void Setup()
        {
            _drivers = new List<Driver>();
            Random rnd = new Random(51);
            
            for (int i = 0; i < DriversCount; i++)
            {
                _drivers.Add(new Driver(i, rnd.Next(MapWidth), rnd.Next(MapHeight)));
            }

            // Заказ в центре
            _orderLocation = new Location(MapWidth / 2, MapHeight / 2);
        }

        [Benchmark]
        public void Simple_Euclidean()
        {
            _simpleSearch.FindDrivers(_orderLocation, _drivers, MapWidth, MapHeight);
        }

        [Benchmark]
        public void City_Manhattan()
        {
            _citySearch.FindDrivers(_orderLocation, _drivers, MapWidth, MapHeight);
        }

        [Benchmark]
        public void Square_ExpandingSquare()
        {
            _squareSearch.FindDrivers(_orderLocation, _drivers, MapWidth, MapHeight);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TaxiBenchmarks>();
            Console.ReadLine();
        }
    }
}
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TaxiService;

namespace TaxiService.Tests
{
    [TestFixture]
    public class SearchAlgorithmsTests
    {
        private List<Driver> _drivers;
        private Location _orderLocation;
        private const int Width = 100;
        private const int Height = 100;

        [SetUp]
        public void Setup()
        {
            _drivers = new List<Driver>();
            _orderLocation = new Location(0, 0);
        }

        private void AddDriver(int id, int x, int y)
        {
            _drivers.Add(new Driver(id, x, y));
        }

        // тесты для SimpleSearch (через евклидово расстояние)
        [Test]
        public void SimpleSearch_ShouldReturnDrivers_SortedByEuclideanDistance()
        {
            var search = new SimpleSearch();
            _orderLocation = new Location(0, 0);

            AddDriver(1, 3, 4); // Расстояние = 5
            AddDriver(2, 0, 6); // Расстояние = 6
            AddDriver(3, 1, 1); // Расстояние = 1.41

            var result = search.FindDrivers(_orderLocation, _drivers, Width, Height);

            Assert.That(result.Count, Is.EqualTo(3), "Должно быть найдено 3 водителя");
            Assert.That(result[0].Id, Is.EqualTo(3), "Driver 3 (1,1) должен быть первым");
            Assert.That(result[1].Id, Is.EqualTo(1), "Driver 1 (3,4) должен быть вторым");
            Assert.That(result[2].Id, Is.EqualTo(2), "Driver 2 (0,6) должен быть третьим");
        }

        // Тесты для CitySearch (через "манхэттенское расстояние")
        [Test]
        public void CitySearch_ShouldReturnDrivers_SortedByManhattanDistance()
        {
            var search = new CitySearch();
            _orderLocation = new Location(0, 0);

            AddDriver(1, 3, 4); // Расстояние = 7
            AddDriver(2, 0, 6); // Расстояние = 6

            var result = search.FindDrivers(_orderLocation, _drivers, Width, Height);

            Assert.That(result[0].Id, Is.EqualTo(2), "Driver 2 (0,6) должен быть первым");
            Assert.That(result[1].Id, Is.EqualTo(1), "Driver 1 (3,4) должен быть вторым");
        }

        // Тесты для SquareSearch (в расширяющихся квадратах)
        [Test]
        public void SquareSearch_ShouldReturnDrivers_SortedBySquareLayers()
        {
            var search = new SquareSearch();
            _orderLocation = new Location(0, 0);

            AddDriver(1, 5, 0); // Расстояние = 5
            AddDriver(2, 4, 4); // Расстояние = 4

            var result = search.FindDrivers(_orderLocation, _drivers, Width, Height);

            Assert.That(result[0].Id, Is.EqualTo(2), "Driver 2 (4,4) должен быть первым");
            Assert.That(result[1].Id, Is.EqualTo(1), "Driver 1 (5,0) должен быть вторым");
        }


        // тест на обработку пустого списка водителей
        [Test]
        public void AllAlgorithms_ShouldHandle_EmptyDriverList()
        {
            var algorithms = new object[] { new SimpleSearch(), new CitySearch(), new SquareSearch() };
            
            var emptyList = new List<Driver>();
            
            foreach (dynamic algo in algorithms)
            {
                List<Driver> drivers = algo.FindDrivers(_orderLocation, emptyList, Width, Height);
                
                if (drivers == null)
                {
                    Assert.Fail($"Алгоритм {algo.GetType().Name} вернул null вместо пустого списка");
                }
                
                if (drivers.Count != 0)
                {
                    Assert.Fail($"Алгоритм {algo.GetType().Name} вернул {drivers.Count} водителей вместо 0");
                }
            }
        }

        // все алгоритмы должны возвращать максимум 5 водителей
        [Test]
        public void AllAlgorithms_ShouldReturn_Max5Drivers()
        {
            for (int i = 0; i < 10; i++) AddDriver(i, i, i);

            var algos = new object[] { new SimpleSearch(), new CitySearch(), new SquareSearch() };

            foreach (dynamic algo in algos)
            {
                List<Driver> result = algo.FindDrivers(_orderLocation, _drivers, Width, Height);
                Assert.That(result.Count, Is.EqualTo(5), $"Алгоритм {algo.GetType().Name} вернул неверное количество водителей");
            }
        }

        // тест, что если водителей меньше 5, возвращаются все
        [Test]
        public void AllAlgorithms_ShouldReturn_AllDrivers_IfLessThan5Exist()
        {
            AddDriver(1, 10, 10);
            AddDriver(2, 20, 20);
            AddDriver(3, 30, 30);

            var algos = new object[] { new SimpleSearch(), new CitySearch(), new SquareSearch() };

            foreach (dynamic algo in algos)
            {
                List<Driver> result = algo.FindDrivers(_orderLocation, _drivers, Width, Height);
                Assert.That(result.Count, Is.EqualTo(3), $"Алгоритм {algo.GetType().Name} должен вернуть всех доступных водителей");
            }
        }

        // тест на отсутствие дубликатов в SquareSearch
        [Test]
        public void SquareSearch_ShouldNotReturnDuplicates()
        {
            var search = new SquareSearch();
            AddDriver(1, 1, 1); 
            AddDriver(2, 0, 5); 

            var result = search.FindDrivers(new Location(0,0), _drivers, Width, Height);

            Assert.That(result.Count(d => d.Id == 1), Is.EqualTo(1), "Водитель должен быть добавлен только один раз");
        }
    }
}
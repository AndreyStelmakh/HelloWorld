using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SomeLibraries;



namespace VendingMachineServiceTests
{
    [TestClass]
    public class CoinsUnitTest
    {
        [TestMethod]
        public void CalculateChangeMethod1()
        {
            // в кошеле одна 10руб, три 5-ти руб и восемь 2-х руб
            Wallet wallet = new Wallet(new CoinPack[] { new CoinPack(10, 1), new CoinPack(5, 3), new CoinPack(2, 8) });

            var result = wallet.CalculateChange<LessCoinsAlgorithm>(11).ToArray();

            Assert.IsTrue(2 == result.Count());
            Assert.IsTrue(1 == result.Count(p => p.Nominal == 2 && p.Quantity == 3));
            Assert.IsTrue(1 == result.Count(p => p.Nominal == 5 && p.Quantity == 1));

        }

        [TestMethod]
        public void CalculateChangeTestMethod2()
        {
            // в кошеле одна 10руб, три 5-ти руб и восемь 2-х руб и 1 руб
            Wallet wallet = new Wallet(new CoinPack[] { new CoinPack(10, 1), new CoinPack(5, 3), new CoinPack(2, 8), new CoinPack(1, 1) });

            var result = wallet.CalculateChange<LessCoinsAlgorithm>(11).ToArray();

            Assert.IsTrue(2 == result.Count());
            Assert.IsTrue(0 != result.Count(p => p.Nominal == 10 && p.Quantity == 1));
            Assert.IsTrue(0 != result.Count(p => p.Nominal == 1 && p.Quantity == 1));

        }

    }

    [TestClass]
    public class ArithmeticUnitTest
    {
        [TestMethod]
        public void DigitTestMethod()
        {
            var q = new ArithmeticDigit(3);

            bool cDigit;
            uint value;

            value = q.CurrentValue();
            Assert.IsTrue(value == 3);

            value = q.NextValue(out cDigit);
            Assert.IsTrue(value == 2);
            Assert.IsTrue(cDigit == false);

            value = q.NextValue(out cDigit);
            Assert.IsTrue(value == 1);
            Assert.IsTrue(cDigit == false);

            value = q.NextValue(out cDigit);
            Assert.IsTrue(value == 0);
            Assert.IsTrue(cDigit == false);

            value = q.CurrentValue();
            Assert.IsTrue(value == 0);

            value = q.NextValue(out cDigit);
            Assert.IsTrue(value == 3);
            Assert.IsTrue(cDigit == true);

            value = q.NextValue(out cDigit);
            Assert.IsTrue(value == 2);
            Assert.IsTrue(cDigit == false);

        }

        [TestMethod]
        public void ArithmeticRegisterTestMethod()
        {
            var list = new System.Collections.Generic.List<uint[]>(SomeLibraries.ArithmeticRegister.AllCombinationsDescending(new uint[] { 1, 2, 1 }));

            string message = string.Empty;
            foreach (var combination in list)
            {
                message += string.Format("\r\n{2}{1}{0}", combination[0], combination[1], combination[2]);
            }

            Assert.IsTrue(list.Count == 12, message);

            Assert.AreEqual<uint>(list[0][0], 1, message);
            Assert.AreEqual<uint>(list[0][1], 2, message);
            Assert.AreEqual<uint>(list[0][2], 1, message);

            Assert.AreEqual<uint>(list[1][0], 0, message);
            Assert.AreEqual<uint>(list[1][1], 2, message);
            Assert.AreEqual<uint>(list[1][2], 1, message);

            Assert.AreEqual<uint>(list[2][0], 1, message);
            Assert.AreEqual<uint>(list[2][1], 1, message);
            Assert.AreEqual<uint>(list[2][2], 1, message);

        }

    }

    [TestClass]
    public class ServiceUnitTest
    {
        enum Товары
        {
            Чай = 0,
            Кофе = 1,
            Кофе_с_молоком = 3,
            Сок = 4
        }

        /// <summary>
        /// Инициализатор сервис в соответствии с условиями задания
        /// </summary>
        /// <returns></returns>
        static VendingMachineService.Service1 GetInitializedServiceInstance()
        {
            #region
            var content = new VendingMachineService.VendingMachineContent();
            content.Add((int)Товары.Чай, "Чай", 13, 10);
            content.Add((int)Товары.Кофе, "Кофе", 18, 20);
            content.Add((int)Товары.Кофе_с_молоком, "Кофе с молоком", 21, 20);
            content.Add((int)Товары.Сок, "Сокъ", 35, 15);
            #endregion

            #region
            var userWallet = new Wallet();
            userWallet.Add(1, 10);
            userWallet.Add(2, 30);
            userWallet.Add(5, 20);
            userWallet.Add(10, 15);
            #endregion

            #region
            var machineWallet = new Wallet();
            machineWallet.Add(1, 100);
            machineWallet.Add(2, 100);
            machineWallet.Add(5, 100);
            machineWallet.Add(10, 100);
            #endregion

            var service = new VendingMachineService.Service1();

            service.Initialize(content, machineWallet, userWallet);

            return service;

        }

        [TestMethod]
        public void InitializeTestMethod()
        {
            var service = GetInitializedServiceInstance();

            Assert.IsTrue(4 == service.GetVendingMachineContent().Count(), string.Format("0"));
            Assert.IsTrue(1800 == service.GetVendingMachineWallet().Sum(p => p.Nominal * p.Quantity), string.Format("1"));
            Assert.IsTrue(320 == service.GetUserWallet().Sum(p => p.Nominal * p.Quantity), string.Format("2"));

        }

        [TestMethod]
        public void SimpleSequenceTestMethod()
        {
            var service = GetInitializedServiceInstance();

            #region
            Assert.IsTrue(1 == service.GetUserWallet().Count(p => p.Nominal == 10), string.Format("{0}", service.GetUserWallet().Count(p => p.Nominal == 10)));
            Assert.IsTrue(1 == service.GetUserWallet().Count(p => p.Nominal == 5));
            Assert.IsTrue(1 == service.GetUserWallet().Count(p => p.Nominal == 2));
            Assert.IsTrue(1 == service.GetUserWallet().Count(p => p.Nominal == 1));

            Assert.IsTrue(1 == service.GetVendingMachineContent().Count(p => p.Id == 0 && p.Price == 13 && p.Quantity == 10 && p.Name.Equals("Чай")));
            #endregion

            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);
            service.MoveCoinInCash(10);

            Assert.IsTrue(1 == service.GetUserWallet().Count(p => p.Nominal == 10 && p.Quantity == 7));

            Assert.IsTrue(80 == service.GetUsersMoneyInCash());

            #region
            service.BuyOne((int)Товары.Кофе_с_молоком);
            service.BuyOne((int)Товары.Кофе_с_молоком);
            service.BuyOne((int)Товары.Кофе_с_молоком);
            service.BuyOne((int)Товары.Чай);
            #endregion

            Assert.IsTrue(4 == service.GetUsersMoneyInCash());

            service.GetChange();

            Assert.IsTrue(1876 == service.GetVendingMachineWallet().Sum(p => p.Nominal * p.Quantity));

            //вернёт двушками
            Assert.IsTrue(1 == service.GetUserWallet().Count(p => p.Nominal == 2 && p.Quantity == 32));

        }

    }

}

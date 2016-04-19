using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace VendingMachineService
{
    [ServiceContract(SessionMode=SessionMode.Allowed)]
    public interface IService1
    {
        /// <summary>
        /// Просмотр содержимого кошелька покупателя
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DataContracts.CoinPackDataContract[] GetUserWallet();

        /// <summary>
        /// Просмотр товаров машины
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DataContracts.ItemTypeDataContract[] GetVendingMachineContent();

        /// <summary>
        /// Просмотр содержимого кошелька машины
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DataContracts.CoinPackDataContract[] GetVendingMachineWallet();

        /// <summary>
        /// Количество денег покупателя в монетоприемнике (часть из этих денег покупателю может не принадлежать после покупки)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        uint GetUsersMoneyInCash();

        /// <summary>
        /// Команда пользователя "Поместить монету в монетоприемник"
        /// </summary>
        /// <param name="nominal"></param>
        [OperationContract]
        void MoveCoinInCash(uint nominal);

        /// <summary>
        /// Команда пользователя "Выдать сдачу"
        /// </summary>
        [OperationContract]
        void GetChange();

        /// <summary>
        /// Команда пользователя "Приобрести еденицу товара"
        /// </summary>
        /// <param name="id"></param>
        [OperationContract]
        void BuyOne(int id);

    }

}

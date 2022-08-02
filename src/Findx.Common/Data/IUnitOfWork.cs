﻿using System;
using System.Data.Common;

namespace Findx.Data
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// 获取 是否已提交
        /// </summary>
        bool HasCommitted { get; }
        
        /// <summary>
        /// 启用事务，事务代码写在 UnitOfWork.EnableTransaction() 与 UnitOfWork.Commit() 之间
        /// </summary>
        void EnableTransaction();

        /// <summary>
        /// 获取 是否启用事务
        /// </summary>
        bool IsEnabledTransaction { get; }
        
        /// <summary>
		/// 数据库连接
		/// </summary>
		DbConnection Connection { get; }

        /// <summary>
        /// 工作单元事务
        /// </summary>
        DbTransaction Transaction { get; set; }

        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginOrUseTransaction();

        /// <summary>
        /// 提交
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚
        /// </summary>
        void Rollback();
    }
    /// <summary>
    /// 泛型工作单元
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWork<T> : IUnitOfWork
    {
        /// <summary>
        /// 获取泛型实例
        /// </summary>
        /// <returns></returns>
        T GetInstance();
    }
}

// ************************************************************
//
// Copyright (c) 2016 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

using Professional_Gift_Card_System;

namespace Professional_Gift_Card_System.Models
{

    // EF follows a Code based Configuration model and will look for a class that
    // derives from DbConfiguration for executing any Connection Resiliency strategies
    public class EFConfiguration : DbConfiguration
    {
        public EFConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy()); 
            //AddExecutionStrategy(() => new SqlAzureExecutionStrategy());
        }
    }
    // this is an attempt to make a repository base class that could be used more generically
    // A repository is the interface between the business case and the database

    // another generic repository is at 
    // http://huyrua.wordpress.com/2010/07/13/entity-framework-4-poco-repository-and-specification-pattern/


    public interface IDAO<TDataEntity> where TDataEntity : class
    {
        //IQueryable<T> List<T>() where T : class;
        //T Get<T>(int id) where T : class;
        //void Create<T>(T entityTOCreate) where T : class;
        //void Edit<T>(T entityToEdit) where T : class;
        //void Delete<T>(T entityToDelete) where T : class;
        //void Add(TDataEntity entity);
        //void Delete(TDataEntity entity); 
        void SaveChanges();

    }

    public abstract class BaseDAO<TDataEntity> : IDAO<TDataEntity> where TDataEntity : class
    {
        protected GiftEntities GiftEntity;

        /// <summary>
        /// The context object for the database
        /// </summary>
        /// not used in this version of it.
        //private ObjectContext _context;

        /// <summary>
        /// The IObjectSet that represents the current entity.
        /// </summary>

        
        
        //id property of our database table we'll be obtaining.
        const string keyPropertyName = "ID";


        public BaseDAO() { }
        public BaseDAO(GiftEntities PassedGiftEntity)
        {
            GiftEntity = PassedGiftEntity;
        }


        // I want to do this in classes that inherit from this
        //IQueryable<T> List<T>() where T : class;
        //T Get<T>(int id) where T : class;
        //void Create<T>(T entityTOCreate) where T : class;
        //void Edit<T>(T entityToEdit) where T : class;

        protected void InitializeConnection()
        {
            if (GiftEntity == null)
                GiftEntity = new GiftEntities();
        }
        public GiftEntities CreateNew()
        {
            GiftEntity = new GiftEntities();
            return GiftEntity;
        }



        /// <summary>
        /// Saves all the changes
        /// </summary>
        public void SaveChanges()
        {
            GiftEntity.SaveChanges();
        }

    }
}
﻿using DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Factories
{
    public class LePadContextFactory : IDbContextFactory<LePadContext>
    {
        public LePadContext Create(DbContextFactoryOptions options)
        {
            DbContextOptionsBuilder<LePadContext> builder = new DbContextOptionsBuilder<LePadContext>();

            builder.UseSqlServer("Server=TIMOTHY-PC;Database=le_pad_db;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new LePadContext(builder.Options);
        }
    }
}

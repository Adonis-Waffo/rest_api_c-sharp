﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenImis.DB.SqlServer;
using OpenImis.ModulesV2.InsureeModule.Helpers;
using OpenImis.ModulesV2.InsureeModule.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Globalization;

namespace OpenImis.ModulesV2.InsureeModule.Repositories
{
    public class InsureeRepository : IInsureeRepository
    {
        private IConfiguration _configuration;

        public InsureeRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GetEnquireModel GetEnquire(string chfid)
        {
            GetEnquireModel response = new GetEnquireModel();
            List<DetailModel> details = new List<DetailModel>();

            try
            {
                using (var imisContext = new ImisDB())
                {
                    var chfidParameter = new SqlParameter("@CHFID", chfid) { SqlDbType = SqlDbType.VarChar, Size = 12 };

                    var sql = "exec uspPolicyInquiry @CHFID";

                    DbConnection connection = imisContext.Database.GetDbConnection();

                    using (DbCommand cmd = connection.CreateCommand())
                    {

                        cmd.CommandText = sql;

                        cmd.Parameters.AddRange(new[] { chfidParameter });

                        if (connection.State.Equals(ConnectionState.Closed)) connection.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            do
                            {
                                bool firstValue = true;
                                while (reader.Read())
                                {
                                    if (firstValue)
                                    {
                                        response.CHFID = reader["CHFID"].ToString();
                                        response.PhotoPath = reader["PhotoPath"].ToString();
                                        response.InsureeName = reader["InsureeName"].ToString();
                                        response.DOB = reader["DOB"].ToString();
                                        response.Gender = reader["Gender"].ToString();

                                        firstValue = false;
                                    }

                                    details.Add(new DetailModel
                                    {
                                        ProductName = reader["ProductName"].ToString(),
                                        ExpiryDate = reader["ExpiryDate"].ToString(),
                                        Status = reader["Status"].ToString(),
                                        DedType = reader["DedType"].ToString().ToNullableFloat(),
                                        Ded1 = reader["Ded1"].ToString().ToNullableDecimal(),
                                        Ded2 = reader["Ded2"].ToString().ToNullableDecimal(),
                                        Ceiling1 = reader["Ceiling1"].ToString().ToNullableDecimal(),
                                        Ceiling2 = reader["Ceiling2"].ToString().ToNullableDecimal(),
                                        ProductCode = reader["ProductCode"].ToString()
                                    });
                                }
                            } while (reader.NextResult());
                        }
                    }
                }

                response.Details = details;

                return response;
            }
            catch (SqlException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public GetInsureeModel Get(string chfid)
        {
            GetInsureeModel response = new GetInsureeModel();

            try
            {
                using (var imisContext = new ImisDB())
                {
                    response = (from I in imisContext.TblInsuree
                                join P in imisContext.TblPhotos on I.Chfid equals P.Chfid
                                join G in imisContext.TblGender on I.Gender equals G.Code
                                where I.Chfid == chfid
                                && I.ValidityTo == null
                                && P.ValidityTo == null
                                select new GetInsureeModel()
                                {
                                    CHFID = I.Chfid,
                                    DOB = I.Dob.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                    Gender = G.Gender,
                                    InsureeName = I.LastName + " " + I.OtherNames,
                                    PhotoPath = P.PhotoFolder + P.PhotoFileName
                                })
                             .FirstOrDefault();
                }

                return response;
            }
            catch (SqlException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

using LegacyApp.Repositories;

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace LegacyApp.Repositories
{
    public class UserRepository:IUserRepository
    {
        public void AddUser(User user)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["appDatabase"].ConnectionString;

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = "uspAddUser"
            };

            var firstNameParameter = new SqlParameter("@Firstname", SqlDbType.VarChar, 50) { Value = user.Firstname };
            var surnameParameter = new SqlParameter("@Surname", SqlDbType.VarChar, 50) { Value = user.Surname };
            var dateOfBirthParameter = new SqlParameter("@DateOfBirth", SqlDbType.DateTime) { Value = user.DateOfBirth };
            var emailAddressParameter = new SqlParameter("@EmailAddress", SqlDbType.VarChar, 50) { Value = user.EmailAddress };
            var hasCreditLimitParameter = new SqlParameter("@HasCreditLimit", SqlDbType.Bit) { Value = user.HasCreditLimit };
            var creditLimitParameter = new SqlParameter("@CreditLimit", SqlDbType.Int) { Value = user.CreditLimit };
            var clientIdParameter = new SqlParameter("@ClientId", SqlDbType.Int) { Value = user.Client.Id };

            command.Parameters.AddRange(new[] { firstNameParameter, surnameParameter, dateOfBirthParameter,
                    emailAddressParameter, hasCreditLimitParameter, creditLimitParameter, clientIdParameter });

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
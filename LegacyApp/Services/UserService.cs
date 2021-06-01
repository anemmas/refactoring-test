using LegacyApp.Repositories;

using System;
using System.Text.RegularExpressions;

namespace LegacyApp
{
    public class UserService
    {
        private readonly IClientRepository clientRepository;
        private readonly IUserCreditService userCreditServiceClient;
        private readonly IUserRepository userRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        public UserService()
            : this(new ClientRepository(), new UserCreditServiceClient(), new UserRepository())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService" /> class.
        /// </summary>
        /// <param name="clientRepository">The client repository.</param>
        /// <param name="userCreditServiceClient">The user credit service client.</param>
        /// <param name="userRepository">The user repository.</param>
        public UserService(IClientRepository clientRepository, IUserCreditService userCreditServiceClient, IUserRepository userRepository)
        {
            this.clientRepository = clientRepository;
            this.userCreditServiceClient = userCreditServiceClient;
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Adds the user.
        /// </summary>
        /// <param name="firstName">The firstName.</param>
        /// <param name="surname">The surname.</param>
        /// <param name="email">The email.</param>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>True if the user has been created; otherwise false.</returns>
        public bool AddUser(string firstName, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(surname)
                || !ValidateEmail(email)
                || !ValidateAge(dateOfBirth))
            {
                return false;
            }


            var client = this.clientRepository.GetById(clientId);
            var user = CreateUser(firstName, surname, email, dateOfBirth, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            this.userRepository.AddUser(user);

            return true;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="firname">The firname.</param>
        /// <param name="surname">The surname.</param>
        /// <param name="email">The email.</param>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <param name="client">The client.</param>
        /// <returns>A new instance of user</returns>
        private User CreateUser(string firname, string surname, string email, DateTime dateOfBirth, Client client)
        {
            var hasCreditLimit = client.Name != "VeryImportantClient";
            var doubleLimitAmount = client.Name == "ImportantClient";

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname,
                HasCreditLimit = hasCreditLimit,
            };

            CalculateCredit(user, hasCreditLimit, doubleLimitAmount);
            return user;
        }


        /// <summary>
        /// Validates the email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>True if the email is valid; otherwise false</returns>
        private static bool ValidateEmail(string email)
        {
            var regex = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            return Regex.IsMatch(email, regex);
        }

        /// <summary>
        /// Calculates the credit limit if necessary.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="hasCreditLimit">if set to <c>true</c> [has credit limit].</param>
        /// <param name="doubleLimitAmount">if set to <c>true</c> [double limit amount].</param>
        private void CalculateCredit(User user, bool hasCreditLimit, bool doubleLimitAmount)
        {
            if (!hasCreditLimit)
            {
                return;
            }

            var creditLimit = this.userCreditServiceClient.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
            user.CreditLimit = doubleLimitAmount ? creditLimit * 2 : creditLimit;
        }

        /// <summary>
        /// Validates the user age.
        /// </summary>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <returns>True if dateOfBirth + 21 years is lower than DateTime.Today</returns>
        private static bool ValidateAge(DateTime dateOfBirth)
        {
            return dateOfBirth.AddYears(21).AddDays(-1) < DateTime.Today;
        }
    }
}
using AdminClient.ServiceReference1;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AdminClient
{
    class Program
    {
        public static bool Active { get; set; }

        static void Main(string[] args)
        {
            ServiceClient client = new ServiceClient();
            Active = true;

            while (Active)
            {
                int option = MainMenu();
                ProcessOption(option, client);
            }

        }

        #region MainMenu
        private static int MainMenu()
        {
            Console.WriteLine("");
            Console.WriteLine("Menu Principal:");
            Console.WriteLine("1. Alta de usuario");
            Console.WriteLine("2. Baja de usuario");
            Console.WriteLine("3. Modificar usuario");
            Console.WriteLine("4. Salir");
            Console.WriteLine("Seleccione la opcion que desea realizar");

            string line = Console.ReadLine();
            int option = ConvertToInt(line);
            while (!(option <= 4 && option > 0) || !IsValidOption(line))
            {
                Console.WriteLine("Opcion no valida, seleccione una opcion correcta");
                line = Console.ReadLine();
                option = ConvertToInt(line);
            }
            return option;
        }

        public static bool IsValidOption(string word)
        {
            if (!(word.Length == 1))
            {
                return false;
            }
            string options = "1234";
            if (options.Contains(word))
            {
                return true;
            }
            return false;
        }

        public static int ConvertToInt(string line)
        {
            if (IsValidOption(line))
            {
                return Convert.ToInt32(line);
            }
            else
            {
                return 1000;
            }
        }
        #endregion

        private static void ProcessOption(int option, ServiceClient client)
        {
            switch (option)
            {
                case 1:
                    RegisterUser(client);
                    break;
                case 2:
                    DeleteUser(client);
                    break;
                case 3:
                    ModifyUser(client);
                    break;
                case 4:
                    CloseApp();
                    break;
            }
        }

        private static void RegisterUser(ServiceClient client)
        {
            string name = "";
            string pass = "";
            bool datoValidoUser = false;
            while (!datoValidoUser)
            {
                Console.WriteLine("Ingrese el nombre de usuario a registrar: ");
                name = Console.ReadLine();
                if (name.Length > 0)
                {
                    datoValidoUser = true;
                }
            }


            bool datoValidoPass = false;
            while (!datoValidoPass)
            {
                Console.WriteLine("Ingrese la contraseña: ");
                pass = Console.ReadLine();
                if (pass.Length > 0)
                {
                    datoValidoPass = true;
                }
            }

            bool serverResponse = client.RegisterUser(name, pass);

            if (serverResponse.Equals(true))
            {
                Console.WriteLine("Usuario registrado con exito.");
                

            }
            else
            {
                Console.WriteLine("No se pudo registrar, el usuario ya exite o algun dato esta vacio.");
            }
        }

        private static void DeleteUser(ServiceClient client)
        {
            Console.WriteLine("Lista de usuarios: ");

            List<User> users = client.GetUsers().ToList();

            int cont = 1;
            foreach (var user in users)
            {
                Console.WriteLine("{0}. " + user.UserName, cont);
            }

            Console.WriteLine("Escriba el nombre del usuario que desea eliminar: ");
            string toDelete = Console.ReadLine();

            bool serverResponse = client.DeleteUser(toDelete);

            if (serverResponse)
            {
                Console.WriteLine("Usuario eliminado con exito.");
                
            }
            else
            {
                Console.WriteLine("No se pudo eliminar, el usuario no existe o esta conectado.");
            }
        }

        private static void ModifyUser(ServiceClient client)
        {
            Console.WriteLine("Lista de usuarios: ");

            List<User> users = client.GetUsers().ToList();

            int cont = 1;
            foreach (var user in users)
            {
                Console.WriteLine("{0}. " + user.UserName, cont);
            }

            Console.WriteLine("Escriba el nombre del usuario que desea modificar: ");
            string toModify = Console.ReadLine();

            Console.WriteLine("Escriba el nuevo nombre de usuario: ");
            string newName = Console.ReadLine();

            Console.WriteLine("Escriba la nueva contraseña: ");
            string newPass = Console.ReadLine();

            bool serverResponse = client.ModifyUser(toModify, newName, newPass);

            if (serverResponse)
            {
                Console.WriteLine("Usuario modificado con exito.");
                
            }
            else
            {
                Console.WriteLine("No se pudo modificar, el usuario no exite o esta conectado.");
            }
        }

        private static void CloseApp()
        {
            Environment.Exit(0);
        }
      
    }
}

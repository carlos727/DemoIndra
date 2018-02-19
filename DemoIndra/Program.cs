using System;
using Hyland.Unity;
using Hyland.Unity.Extensions;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using DemoIndra.Core;

namespace DemoIndra
{
    class Program
    {
        static void Main(string[] args)
        {
            Configuration Config = new Configuration(@"C:\UploadToOnbase\config.xml");

            Application indra_app = ConnectToOnBase(Config.URL, Config.USERNAME, Config.PASSWORD, Config.DATASRC);

            if (indra_app == null)
            {
                throw new Exception("Hubo un error intentando iniciar sesión. Por favor contacta tu administrador de OnBase.");
            }
            Console.WriteLine("Conexión establecida...");

            if (!File.Exists(Config.FILEPATH))
            {
                throw new Exception("El archivo " + Config.FILEPATH + " no existe.");
            }
            Console.WriteLine("Archivo listo para ser leído...");

            StreamReader fileContent = new StreamReader(Config.FILEPATH);
            string line;
            while ((line = fileContent.ReadLine()) != null)
            {
                // Valores de la line x del archivo fuente
                string[] parameters = line.Replace(Environment.NewLine, string.Empty).Split(Config.SEPARATOR);

                // Diccionario keyword: valor
                Dictionary<string,string> keywords = new Dictionary<string, string>();
                int i = 0;
                while (i < parameters.Length)
                {
                    keywords.Add(Config.ORDER[i], parameters[i]);
                    i++;
                }

                if (File.Exists(keywords["nombre_archivo"]))
                {
                    long document = UploadNewDocuent(indra_app, Config.DOCTYPE, keywords);
                    Console.WriteLine("Nuevo documento importado a OnBase ID: " + document);                    
                }
                else
                {
                    Console.WriteLine("El archivo " + keywords["nombre_archivo"] + " no existe.");
                }                
            }
            fileContent.Close();    

            DisconnectFromOnBase(indra_app);

            Console.ReadKey();
        }

        static Application ConnectToOnBase(string url, string user, string pwd, string dataSource)
        {
            Application Application = null;

            try
            {
                //Connect the Application Object  
                OnBaseAuthenticationProperties props = Application.CreateOnBaseAuthenticationProperties(url, user, pwd, dataSource);
                Application = Application.Connect(props);
            }
            catch (InvalidLoginException ex)
            {
                throw new Exception("The credentials entered are invalid.", ex);
            }
            catch (AuthenticationFailedException ex)
            {
                throw new Exception("Authentication failed.", ex);
            }
            catch (MaxConcurrentLicensesException ex)
            {
                throw new Exception("All licenses are currently in use, please try again later.", ex);
            }
            catch (NamedLicenseNotAvailableException ex)
            {
                throw new Exception("Your license is not availble, please insure you are logged out of other OnBase clients.", ex);
            }
            catch (SystemLockedOutException ex)
            {
                throw new Exception("The system is currently locked, please try back later.", ex);
            }
            catch (UnityAPIException ex)
            {
                throw new Exception("There was an unhandled exception with the Unity API.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("There was an unhandled exception.", ex);
            }

            return Application;
        }

        static Application DisconnectFromOnBase(Application app)
        {
            try
            {
                // Disconnect Code Required Here
                if (app != null)
                {
                    app.Dispose();
                    Console.WriteLine("Conexión finalazada.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return app;
        }

        static long UploadNewDocuent(Application app, string docType, Dictionary<string, string> keywords)
        {
            try
            {
                long newDocID = -1;
                using (PageData pageData = app.Core.Storage.CreatePageData(keywords["archivo"]))
                {
                    // Get Document Type
                    DocumentType documentType = app.Core.DocumentTypes.Find(docType);
                    if (documentType == null)
                    {
                        throw new Exception("Tipo documental no encontrado: " + docType);
                    }

                    // Get File Type
                    FileType imageFileType = app.Core.FileTypes.Find("Image File Format");
                    if (imageFileType == null)
                    {
                        throw new Exception("No se pudo encontrar el formato de archivo: Image File Format");
                    }

                    //Create Document Properties Object
                    StoreNewDocumentProperties newDocProps = app.Core.Storage.CreateStoreNewDocumentProperties(documentType, imageFileType);

                    foreach(string key in keywords.Keys)
                    {
                        if (!key.Equals("nombre_archivo"))
                        {
                            //Get Keyword Types
                            KeywordType keyType = app.Core.KeywordTypes.Find(key);
                            if (keyType == null)
                            {
                                throw new Exception("No se pudo encontrar el keyword type: " + key);
                            }

                            // Create Keyword Objects
                            Keyword keyword = null;
                            if (!keyType.TryCreateKeyword(keywords[key], out keyword))
                            {
                                throw new Exception(key + " no pudo ser creado.");
                            }

                            // Add the new keywords to our properties.
                            newDocProps.AddKeyword(keyword);
                        }
                    }                   

                    // Create the new document.
                    Document newDocument = app.Core.Storage.StoreNewDocument(pageData, newDocProps);

                    // Set the newDocID to the ID of the newly imported document. 
                    newDocID = newDocument.ID;

                    return newDocID;
                }
            }
            catch (SessionNotFoundException ex)
            {
                app.Diagnostics.Write(ex);
                throw new Exception("The Unity API session could not be found, please reconnect.", ex);
            }
            catch (UnityAPIException ex)
            {
                app.Diagnostics.Write(ex);
                throw new Exception("There was a Unity API exception.", ex);
            }
            catch (Exception ex)
            {
                app.Diagnostics.Write(ex);
                throw new Exception("There was an unknown exception.", ex);
            }
        }
    }
}

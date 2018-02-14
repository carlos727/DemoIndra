using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hyland.Unity;
using Hyland.Unity.Extensions;
using System.IO;

namespace DemoIndra
{
    class Program
    {
        static void Main(string[] args)
        {
            const string URL = "http://192.168.30.192/appserver/service.asmx";
            const string USERNAME = "MANAGER";
            const string PASSWORD = "PASSWORD";
            const string DATA_SRC = "OnBase";
            const string DOCTYPE = "FacturaTest";
            const string KEYWORD1 = "Invoice #";
            const string KEYWORD2 = "Invoice Amount";
            const string KEYWORD3 = "Invoice Date";
            const string FILEPATH = @"C:\UploadToOnbase\upload.txt";
            const string IMAGEDIR = @"C:\UploadToOnbase";
            const char SEPARATOR = ';';
            const string DATEFORMAT = "dd/MM/yyyy";
            Random rand = new Random();
            DateTime date = DateTime.Now;

            Application indra_app = ConnectToOnBase(URL, USERNAME, PASSWORD, DATA_SRC);

            if (indra_app == null)
            {
                throw new Exception("Hubo un error intentando iniciar sesión. Por favor contacta tu administrador de OnBase.");
            }
            Console.WriteLine("Conexión establecida...");

            string[] files = Directory.GetFiles(IMAGEDIR, "*", SearchOption.TopDirectoryOnly);
            files.ToList().ForEach(file => {
                long document = UploadNewDocuent(indra_app, file, DOCTYPE, KEYWORD1, KEYWORD2, KEYWORD3, rand.Next(400, 500), rand.Next(100000, 500000), date.ToString(DATEFORMAT));
                Console.WriteLine("Nuevo documento importado a OnBase ID: " + document);
            });            

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

        static long UploadNewDocuent(Application app, string filePath, string docType, string key1, string key2, string key3, int value1, int value2, string date)
        {
            try
            {
                long newDocID = -1;
                using (PageData pageData = app.Core.Storage.CreatePageData(filePath))
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

                    //Get Keyword Types
                    KeywordType keyType1 = app.Core.KeywordTypes.Find(key1);
                    if (keyType1 == null)
                    {
                        throw new Exception("No se pudo encontrar el keyword type: " + key1);
                    }

                    KeywordType keyType2 = app.Core.KeywordTypes.Find(key2);
                    if (keyType2 == null)
                    {
                        throw new Exception("No se pudo encontrar el keyword type: " + key2);
                    }

                    KeywordType keyType3 = app.Core.KeywordTypes.Find(key3);
                    if (keyType3 == null)
                    {
                        throw new Exception("No se pudo encontrar el keyword type: " + key3);
                    }

                    // Create Keyword Objects
                    Keyword keyword1 = null;
                    if (!keyType1.TryCreateKeyword(value1, out keyword1))
                    {
                        throw new Exception(key1 + " no pudo ser creado.");
                    }

                    Keyword keyword2 = null;
                    if (!keyType2.TryCreateKeyword(value2, out keyword2))
                    {
                        throw new Exception(key2 + " no pudo ser creado.");
                    }

                    Keyword keyword3 = null;
                    if (!keyType3.TryCreateKeyword(date, out keyword3))
                    {
                        throw new Exception(key3 + " no pudo ser creado.");
                    }

                    // Add the new keywords to our properties.
                    newDocProps.AddKeyword(keyword1);
                    newDocProps.AddKeyword(keyword2);
                    newDocProps.AddKeyword(keyword3);

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

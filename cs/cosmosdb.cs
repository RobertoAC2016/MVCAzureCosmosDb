using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System.Net;

namespace MVCConAzure.cs
{
    public class cosmosdb
    {
        private string EndpointUri;
        private string PrimaryKey;
        private string databaseId = "Users";
        private string containerId = "pk";
        private DocumentClient client;
        public cosmosdb(IConfiguration config)
        {
            EndpointUri = config["url"];
            PrimaryKey = config["token"];
            Init();
        }
        private void Init()
        {
            try
            {
                client = new DocumentClient(
                    new Uri(EndpointUri),
                    PrimaryKey
                    );
            }
            catch (DocumentClientException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<user>> get_user_list()                               //Este metodo es para consulta de los usuarios
        {
            var SQL = "Select * from c";                                            //Este es el querie q usamos en azure para consultar los datos
            List<user> users = new List<user>();                                    //Este sera el modelo q contendra la informacion de la BDs
            IDocumentQuery<user> query = client.CreateDocumentQuery<user>(          //Este sera el querie q se ejecutara en azure y traera la info
             UriFactory.CreateDocumentCollectionUri(databaseId, containerId), SQL)  //Esta es la referencia de DB, contenedor y querie
             .AsDocumentQuery();                                                    //Con esta sentencia podemos acceder a los datos devueltos

            while (query.HasMoreResults)                                            //Aqui solo preguntamos si trae info y se la asignamos a users
            {
                users.AddRange(await query.ExecuteNextAsync<user>());               //Aqui asignamos los datos a nuestro modelos
            }
            return users;                                                           //Finalmente regresamos el modelo con datos o vacio en caso 
        }                                                                           //de ser nullo o venir vacio el querie de azure
        //Aqui no hay await por eso el metodo regresa un boleano sin async
        public Boolean add_item(user usr)                                           //Con este metodo agregaremos un nuevo usuario a la BDs
        {
            Boolean insertok = true;
            try
            {
                var db = client.CreateDatabaseQuery().AsEnumerable().FirstOrDefault();  //Aqui hacemos la peticion
                var col = client.CreateDocumentCollectionQuery(db.CollectionsLink).AsEnumerable().FirstOrDefault(); //Aqio pregntamos si existe la referencia para la creacion en la BDs
                var res = client.CreateDocumentAsync(col.DocumentsLink, usr).Result;    //aqui mandamso el registro a la conexion y regresamos la respuesta de q fue exitoso con el estatus de created
                if (!res.StatusCode.Equals(HttpStatusCode.Created)) //Si es estatus no es created, entonces algo paso y el registro no se creo
                {
                    insertok = false;   //Si no se creo, regresamos un false
                }
            }
            catch (DocumentClientException ex)
            {
                insertok = false;
                throw;
            }
            catch (Exception ex)
            {
                insertok = false;
                throw;
            }
            return insertok;
        }
        public async Task<Boolean> delete_item(user usr)    //  Con este metodo eliminaremos un registro de la BDs de azure
        {
            Boolean deleteok = true;
            try
            {
                //Aqui primero preguntamos si el registro existe
                var SQL = $"Select * from c where c.id = '{usr.id}'";
                List<user> users = new List<user>();
                IDocumentQuery<user> query = client.CreateDocumentQuery<user>(
                 UriFactory.CreateDocumentCollectionUri(databaseId, containerId), SQL)
                 .AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    users.AddRange(await query.ExecuteNextAsync<user>());
                }
                var u = users.FirstOrDefault();
                var s = JsonConvert.SerializeObject(u);
                dynamic d = JsonConvert.DeserializeObject(s);
                //si el registro existe, entonces hacemos la peticion para su eliminacion
                var res = await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, containerId, usr.id),
                    new RequestOptions { PartitionKey = PartitionKey.None });
            }
            catch (DocumentClientException ex)
            {
                deleteok = false;
                throw;
            }
            catch (Exception ex)
            {
                deleteok = false;
                throw;
            }
            return deleteok;
        }
        public async Task<Boolean> update_item(user usr)    //Con esta sentencia hacemos la actualizacion de un registro de la Bds
        {
            Boolean updateok = true;
            try
            {
                //Aqui preguntamos si el registro existe y de ser asi, se lo asignamos a la variabla de tipo document
                Document doc = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, containerId, usr.id),
                    new RequestOptions { PartitionKey = PartitionKey.None });
                //Aqui debemos de actualizar el o los datos q deben actualizarse del registro
                //Pero aqui lo hago con un foreach de las propiedade el modelo
                foreach (var n in usr.GetType().GetProperties())
                {
                    if (!n.Name.Equals("id"))//Aqui pongo una condicion para q no se actualice el ID, solo los demas campos del modelo
                    {
                        doc.SetPropertyValue(n.Name, n.GetValue(usr, null));
                    }
                }
                //Finalmente, solo se hace el replace del ID especifico y regresa el estatus OK en caso de ser exitoso, sino es q ocurrio algo
                var resp = await client.ReplaceDocumentAsync(doc,
                    new RequestOptions { PartitionKey = PartitionKey.None });

                if (!resp.StatusCode.Equals(HttpStatusCode.OK))
                {
                    updateok = false;//Regreso false en caso de error en la actualizacion
                }
            }
            catch (DocumentClientException ex)
            {
                updateok = false;
                throw;
            }
            catch (Exception ex)
            {
                updateok = false;
                throw;
            }
            return updateok;
        }
        //Finalmente agrego el metodo de lectura de un registro en especifico
        //Tambien se filtra por el id del registro
        //Todas las peticiones son de tipo async ya q azure utiliza metodos asincronos y por cada await se especifica un metodo async
        public async Task<user> read_item(String id)
        {
            try
            {
                DocumentResponse<user> doc = await client.ReadDocumentAsync<user>(UriFactory.CreateDocumentUri(databaseId, containerId, id),
                    new RequestOptions { PartitionKey = PartitionKey.None });
                return doc.Document;    //Aqui el document es de tipo user, ya q se especifico en el tipo de la variable doc
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

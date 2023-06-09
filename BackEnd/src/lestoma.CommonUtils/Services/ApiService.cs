﻿using lestoma.CommonUtils.DTOs;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Helpers;
using lestoma.CommonUtils.Interfaces;
using lestoma.CommonUtils.Requests;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace lestoma.CommonUtils.Services
{
    public class ApiService : IApiService
    {
        private string _tokenNuevo;
        public ResponseDTO Respuesta { get; set; }

        #region Check conexion para consumo de servicios por internet
        public bool CheckConnection() => Connectivity.NetworkAccess == NetworkAccess.Internet;
        #endregion

        #region Get httpclient

        private static HttpClient GetHttpClient(string urlBase)
        {

            if (urlBase.Contains("https"))
            {
                var httpClientHandler = new HttpClientHandler();

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, errors) =>
                {
                    if (!urlBase.Contains("https")) return true; // for development, trust all certificates
                    return errors == SslPolicyErrors.None;
                };

                return new HttpClient(httpClientHandler)
                {
                    Timeout = TimeSpan.FromSeconds(45),
                    BaseAddress = new Uri(urlBase),
                };
            }

            return new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(45),
                BaseAddress = new Uri(urlBase),
            };
        }

        #endregion

        #region GetList Api service with token

        public async Task<ResponseDTO> GetListAsyncWithToken<T>(string urlBase, string nameService, string token)
        {
            try
            {
                using (var client = GetHttpClient(urlBase))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, nameService)))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await RefreshToken(urlBase);
                            await GetAsyncWithToken(urlBase, nameService, _tokenNuevo);
                        }
                        else if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();

                        T item = JsonConvert.DeserializeObject<T>(jsonString);

                        return item != null
                            ? new ResponseDTO { IsExito = true, Data = item }
                            : new ResponseDTO { IsExito = false, MensajeHttp = "No hay contenido." };
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = ex.Message
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Get By Id Api service with token

        public async Task<ResponseDTO> GetAsyncWithToken(string urlBase, string nameService, string token)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, nameService)))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await RefreshToken(urlBase);
                            await GetAsyncWithToken(urlBase, nameService, _tokenNuevo);
                        }
                        else if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();

                        return JsonConvert.DeserializeObject<ResponseDTO>(jsonString);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = ex.Message
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Get paginado Api service with token

        public async Task<ResponseDTO> GetPaginadoAsyncWithToken<T>(string urlBase, string nameService, string token)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, nameService)))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await RefreshToken(urlBase);
                            await GetPaginadoAsyncWithToken<T>(urlBase, nameService, _tokenNuevo);
                        }
                        else if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);


                        response.EnsureSuccessStatusCode();

                        Paginador<T> item = JsonConvert.DeserializeObject<Paginador<T>>(jsonString);
                        if (item == null)
                        {
                            return new ResponseDTO
                            {
                                IsExito = false,
                                MensajeHttp = "No hay contenido."
                            };
                        }
                        return new ResponseDTO
                        {
                            IsExito = true,
                            Data = item
                        };

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Post Api service

        public async Task<ResponseDTO> PostAsync<T>(string urlBase, string nameService, T model)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using (HttpResponseMessage response = await client.PostAsync(nameService, content))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);

                        if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();
                        return Respuesta;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Post Api service with token
        public async Task<ResponseDTO> PostAsyncWithToken<T>(string urlBase, string nameService, T model, string token)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    using (HttpResponseMessage response = await client.PostAsync(nameService, content))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await RefreshToken(urlBase);
                            await PostAsyncWithToken(urlBase, nameService, model, _tokenNuevo);
                        }
                        else if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();
                        return Respuesta;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Post Api without body service with token
        public async Task<ResponseDTO> PostWithoutBodyAsyncWithToken(string urlBase, string nameService, string token)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    using (HttpResponseMessage response = await client.PostAsync(nameService, null))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);

                        if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();
                        return Respuesta;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Put Api service

        public async Task<ResponseDTO> PutAsync<T>(string urlBase, string nameService, T model)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using (HttpResponseMessage response = await client.PutAsync(nameService, content))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);

                        if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();
                        return Respuesta;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }
        #endregion

        #region Put Api service with token

        public async Task<ResponseDTO> PutAsyncWithToken<T>(string urlBase, string nameService, T model, string token)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    string json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    using (HttpResponseMessage response = await client.PutAsync(nameService, content))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await RefreshToken(urlBase);
                            await PutAsyncWithToken(urlBase, nameService, model, _tokenNuevo);
                        }
                        else if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();
                        return Respuesta;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region Delete Api service with token

        public async Task<ResponseDTO> DeleteAsyncWithToken(string urlBase, string nameService, object id, string token)
        {
            try
            {
                using (HttpClient client = GetHttpClient(urlBase))
                {
                    // Asignar token de autenticación
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                    // Realizar solicitud HTTP DELETE
                    using (HttpResponseMessage response = await client.DeleteAsync($"{nameService}/{id}"))
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await RefreshToken(urlBase);
                            await DeleteAsyncWithToken(urlBase, nameService, id, _tokenNuevo);
                        }
                        else if (!response.IsSuccessStatusCode)
                            return GetResponseError(response);

                        response.EnsureSuccessStatusCode();
                        if (Respuesta == null)
                            return new ResponseDTO
                            {
                                IsExito = true,
                                MensajeHttp = "se ha eliminado correctamente."
                            };
                        return Respuesta;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    MensajeHttp = $"Se ha producido un error en la solicitud HTTP: {ex.Message}"
                };
                throw new HttpRequestException(jsonError.ToString());
            }
            catch (JsonException ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = "Se ha producido un error al deserializar la respuesta JSON."
                };
                throw new JsonException(jsonError.ToString());
            }
            catch (Exception ex)
            {
                LestomaLog.Error(ex.Message);
                var jsonError = new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    MensajeHttp = ex.Message
                };
                throw new Exception(jsonError.ToString());
            }
        }

        #endregion

        #region RefreshToken

        private async Task RefreshToken(string urlBase)
        {
            using (HttpClient client = GetHttpClient(urlBase))
            {
                TipoAplicacionRequest tipoAplicacionRequest = new TipoAplicacionRequest
                {
                    TipoAplicacion = (int)TipoAplicacion.AppMovil
                };
                var json = JsonConvert.SerializeObject(tipoAplicacionRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = await client.PostAsync("Account/refresh-token", content))
                {
                    response.EnsureSuccessStatusCode();
                    string jsonString = await response.Content.ReadAsStringAsync();
                    Respuesta = JsonConvert.DeserializeObject<ResponseDTO>(jsonString);
                    TokenDTO tokenNuevo = (TokenDTO)Respuesta.Data;
                    MovilSettings.Token = JsonConvert.SerializeObject(tokenNuevo);
                    _tokenNuevo = tokenNuevo.Token;
                }
            }
        }

        #endregion

        #region mostrar Mensaje Personalizado para la solicitud del service

        private ResponseDTO GetResponseError(HttpResponseMessage response)
        {

            if (Respuesta == null)
            {
                return new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)response.StatusCode,
                    MensajeHttp = MostrarMensajePersonalizadoStatus(response.StatusCode, string.Empty)
                };
            }

            else if (Respuesta.ErrorsEntries != null)
            {
                return new ResponseDTO
                {
                    IsExito = false,
                    StatusCode = (int)response.StatusCode,
                    MensajeHttp = string.Join("\n\n", Respuesta.ErrorsEntries.Select(i => $"{i.Source}: {i.TitleError}").ToArray())
                };
            }
            return new ResponseDTO
            {
                IsExito = false,
                StatusCode = (int)response.StatusCode,
                MensajeHttp = MostrarMensajePersonalizadoStatus(response.StatusCode, Respuesta.MensajeHttp)
            };
        }
        private string MostrarMensajePersonalizadoStatus(HttpStatusCode statusCode, string mensajeDeLaApi)
        {
            string mensaje = string.Empty;
            if (!string.IsNullOrWhiteSpace(mensajeDeLaApi))
                return mensajeDeLaApi;
            switch ((int)statusCode)
            {

                case (int)HttpStatusCode.Accepted:
                    mensaje = "La solicitud fue aceptada";
                    break;
                case (int)HttpStatusCode.Ambiguous:
                    mensaje = "url ambiguo";
                    break;
                case (int)HttpStatusCode.BadGateway:
                    mensaje = "respuesta no valida";
                    break;
                case (int)HttpStatusCode.BadRequest:
                    mensaje = "La solicitud malformada";
                    break;
                case (int)HttpStatusCode.Conflict:
                    mensaje = "conflicto con el estado actual del server";
                    break;
                case (int)HttpStatusCode.Continue:
                    mensaje = "todo va bien por ahora continua";
                    break;
                case (int)HttpStatusCode.Created:
                    mensaje = "Solicitud con exito y se creo un recurso";
                    break;
                case (int)HttpStatusCode.ExpectationFailed:
                    mensaje = "el expect solicitada no puede ser cumplida";
                    break;
                case (int)HttpStatusCode.Forbidden:
                    mensaje = "no posees los permisos necesarios";
                    break;
                case (int)HttpStatusCode.Found:
                    mensaje = "url cambiado temporalmente";
                    break;
                case (int)HttpStatusCode.GatewayTimeout:
                    mensaje = " tiempo de respuesta null ";
                    break;
                case (int)HttpStatusCode.Gone:
                    mensaje = "contenido borrado  del server";
                    break;
                case (int)HttpStatusCode.HttpVersionNotSupported:
                    mensaje = "el servidor no soporta la version http";
                    break;
                case (int)HttpStatusCode.InternalServerError:
                    mensaje = "error interno del server";
                    break;
                case (int)HttpStatusCode.LengthRequired:
                    mensaje = "rechazo del server por cabecera inadecuada";
                    break;
                case (int)HttpStatusCode.MethodNotAllowed:
                    mensaje = "metodo de solicitud no soportado";
                    break;
                case (int)HttpStatusCode.Moved:
                    mensaje = "peticiones movidas a la url dada";
                    break;
                case (int)HttpStatusCode.NoContent:
                    mensaje = "su peticion no tiene ningun contenido";
                    break;
                case (int)HttpStatusCode.NonAuthoritativeInformation:
                    mensaje = "peticion obtenida de otro server al solicitado";
                    break;
                case (int)HttpStatusCode.NotAcceptable:
                    mensaje = "el servidor no puede responder los datos en ningun valor aceptado";
                    break;
                case (int)HttpStatusCode.NotFound:
                    mensaje = "Petición no encontrada.";
                    break;
                case (int)HttpStatusCode.NotImplemented:
                    mensaje = "el server no soporta alguna funcionalidad";
                    break;
                case (int)HttpStatusCode.NotModified:
                    mensaje = "peticion  o url modificada";
                    break;
                case (int)HttpStatusCode.OK:
                    mensaje = "Solicitud realizada correctamente";
                    break;
                case (int)HttpStatusCode.PartialContent:
                    mensaje = "la peticion serivira parcialmente el contenido solicitado";
                    break;
                case (int)HttpStatusCode.PaymentRequired:
                    mensaje = "este error es ambiguo no esta en uso comuniquese con el ingeniero";
                    break;
                case (int)HttpStatusCode.PreconditionFailed:
                    mensaje = "el server no puede cumplir con alguna condicion impuesta por el navegador en su peticion";
                    break;
                case (int)HttpStatusCode.ProxyAuthenticationRequired:
                    mensaje = "el sever acepta la peticion pero se requiere la autenticacion del proxy";
                    break;
                case (int)HttpStatusCode.RedirectKeepVerb:
                    mensaje = "la información de la solicitud se encuentra en el URI especificado en el encabezado";
                    break;
                case (int)HttpStatusCode.RedirectMethod:
                    mensaje = "rediriguiendo automáticamente al cliente al URI especificado  ";
                    break;
                case (int)HttpStatusCode.RequestedRangeNotSatisfiable:
                    mensaje = "la parte del archivo el server no la tiene ";
                    break;
                case (int)HttpStatusCode.RequestEntityTooLarge:
                    mensaje = "la peticion del navegador es demasiado larga el server no lo procesa";
                    break;
                case (int)HttpStatusCode.RequestTimeout:
                    mensaje = "fallo al continuar la petición";
                    break;
                case (int)HttpStatusCode.RequestUriTooLong:
                    mensaje = "el server no procesa la peticion por lo larga que esta";
                    break;
                case (int)HttpStatusCode.ResetContent:
                    mensaje = "el  request se proceso correctamente pero no devuelve ningun contenido";
                    break;
                case (int)HttpStatusCode.ServiceUnavailable:
                    mensaje = " el servidor no está disponible temporalmente";
                    break;
                case (int)HttpStatusCode.SwitchingProtocols:
                    mensaje = "está cambiando la versión del protocolo o el protocolo.";
                    break;
                case (int)HttpStatusCode.Unauthorized:
                    mensaje = "el recurso solicitado requiere autenticación. ";
                    break;
                case (int)HttpStatusCode.UnsupportedMediaType:
                    mensaje = "indica que la solicitud es de un tipo no admitido.";
                    break;
                case (int)HttpStatusCode.Unused:
                    mensaje = "no esta ulilizado";
                    break;
                case (int)HttpStatusCode.UpgradeRequired:
                    mensaje = " el cliente debe cambiar a un protocolo diferente como TLS / 1.0";
                    break;
                case (int)HttpStatusCode.UseProxy:
                    mensaje = "recurso solicitado solo a travez de proxy";
                    break;
                case 522:
                    mensaje = "Error 522 Se agotó el tiempo de espera, verifique la conexión del servidor.";
                    break;
            }
            return mensaje;
        }

        #endregion
    }
}
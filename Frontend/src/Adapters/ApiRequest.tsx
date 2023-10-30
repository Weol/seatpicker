import Config from "../config";

export default async function ApiRequestJson<T>(method: "POST" | "DELETE" | "GET" | "PUT",
                                                path: string,
                                                token: string | null,
                                                body?: any): Promise<T> {
  const response = await ApiRequest(method, path, token, body);
  return await response.json().then<T>(json => {
    console.log(json)
    return json as T
  })
}

export async function ApiRequest(
  method: "POST" | "DELETE" | "GET" | "PUT",
  path: string,
  token: string | null,
  body?: any): Promise<Response> {
  const headers = new Headers();

  if (token != null) {
    headers.append("Authorization", "Bearer " + token);
  }

  const requestInit: RequestInit = {
    method: method,
    headers: headers,
    redirect: 'follow'
  };

  if (method == "POST" || method == "PUT") {
    headers.append("Content-Type", "text/json");
    if (typeof(body) !== 'undefined') requestInit.body = JSON.stringify(body);
  }

  return new Promise<Response>((resolve, reject) => {
    fetch(Config.ApiBaseUrl + path, requestInit)
      .then(response => {
        console.log(response)
        if (response.status > 299) {
          reject(response)
        } else {
          resolve(response)
        }
      })
      .catch((response) => reject(response))
  })
}

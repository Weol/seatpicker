import Config from "../config";
import {AuthenticationAdapter} from "./AuthenticationAdapter";

export default async function ApiRequestJson<T>(method: "POST" | "DELETE" | "GET" | "PUT",
                                                path: string,
                                                body?: any): Promise<T> {
  const response = await ApiRequest(method, path, body);
  return await response.json()
}

export async function ApiRequest(
  method: "POST" | "DELETE" | "GET" | "PUT",
  path: string,
  body?: any): Promise<Response> {
  return AuthenticationAdapter.getToken()
    .then(async token => {
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
        if (body != null) requestInit.body = JSON.stringify(body);
      }

      return await fetch(Config.ApiBaseUrl + path, requestInit).then<Response>(response => {
        console.log(response)
        console.trace()
        return response
      })
    })
}

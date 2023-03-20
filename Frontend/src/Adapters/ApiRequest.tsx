import Cookies from "universal-cookie";
import Config from "../config";

const cookies = new Cookies();

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
  const token = cookies.get("token");

  const headers = new Headers();
  headers.append("Authorization", "Bearer " + token);

  const requestInit: RequestInit = {
    method: method,
    headers: headers,
    redirect: 'follow'
  };

  if (method == "POST" || method == "PUT") {
    headers.append("Content-Type", "text/json");
    if (body != null) requestInit.body = JSON.stringify(body);
  }

  const response = await fetch(Config.ApiBaseUrl + path, requestInit);

  if (response.ok) return response

  throw response;
}

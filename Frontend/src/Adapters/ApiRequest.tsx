import Config from "../config"
import { AuthAdapter } from "./AuthAdapter"

export default async function ApiRequest(
  method: "POST" | "DELETE" | "GET" | "PUT",
  path: string,
  body?: unknown
): Promise<Response> {
  const headers = new Headers()

  const token = await AuthAdapter.GetToken()
  if (token != null) {
    headers.append("Authorization", "Bearer " + token)
  }

  const requestInit: RequestInit = {
    method: method,
    headers: headers,
    redirect: "follow",
  }

  if (method == "POST" || method == "PUT") {
    headers.append("Content-Type", "application/json")
    if (typeof body !== "undefined") requestInit.body = JSON.stringify(body)
  }

  const response = await fetch(Config.ApiBaseUrl + "/" + path, requestInit)

  const responseClone = response.clone()
  const text = await responseClone.text()
  let logBody
  try {
    logBody = JSON.parse(text)
  } catch {
    logBody = text
  }

  console.log({
    method: method,
    status: response.status,
    url: response.url,
    body: logBody,
  })

  if (response.status === 401) {
    throw response
  } else if (response.status > 499) {
    throw response
  }

  return response
}

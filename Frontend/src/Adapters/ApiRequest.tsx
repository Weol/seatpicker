import Config from "../config"
import { AuthAdapter } from "./AuthAdapter"
import {
  AdapterError,
  AuthenticationError,
  BadRequestError,
  ConflictError,
  InternalServerError,
  NotFoundError,
} from "./AdapterError"
import { useAlerts } from "../Contexts/AlertContext"

export async function ApiRequest(
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

  if (response.status <= 299) return response

  switch (response.status) {
    case 400:
      throw new BadRequestError(400, text ?? response.statusText ?? "Bad request")
    case 405:
      throw new BadRequestError(405, text ?? response.statusText ?? "Bad request")
    case 415:
      throw new BadRequestError(415, text ?? response.statusText ?? "Bad request")
    case 422:
      throw new BadRequestError(422, text ?? response.statusText ?? "Bad request")
    case 404:
      throw new NotFoundError(text ?? response.statusText ?? "Not found")
    case 409:
      throw new ConflictError(text ?? response.statusText ?? "Conflict")
    case 401:
      throw new AuthenticationError(401, text ?? response.statusText ?? "Unauthenticated")
    case 403:
      throw new AuthenticationError(403, text ?? response.statusText ?? "Unauthorized")
  }

  if (response.status > 499) {
    throw new InternalServerError(
      response.status,
      text ?? response.statusText ?? "Internal server error"
    )
  }

  throw new AdapterError(response.status, text ?? response.statusText ?? "Internal server error")
}

export function useHandleAdapterError() {
  const { alertError } = useAlerts()

  return (error: unknown) => {
    if (error instanceof AdapterError) {
      alertError(`Error ${error.code}`, error.message)
    } else {
      throw error
    }
  }
}
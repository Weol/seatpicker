import Config from "../config"
import { useAlerts } from "../Contexts/AlertContext"
import { useAuthenticationAdapter } from "./AuthenticationAdapter"

interface ApiError {
  propertyName: string
  errorMessage: string
  attemptedValue: string
}

export default function useApiRequests() {
  const { getToken } = useAuthenticationAdapter()
  const { alertError } = useAlerts()

  async function apiRequestJson<T>(
    method: "POST" | "DELETE" | "GET" | "PUT",
    path: string,
    body?: unknown
  ): Promise<T> {
    const response = await apiRequest(method, path, body)
    return await response.json().then<T>((json) => {
      console.log(json)
      return json as T
    })
  }

  async function apiRequest(
    method: "POST" | "DELETE" | "GET" | "PUT",
    path: string,
    body?: unknown
  ): Promise<Response> {
    const headers = new Headers()

    const token = await getToken()
    if (token != null) {
      headers.append("Authorization", "Bearer " + token)
    }

    const requestInit: RequestInit = {
      method: method,
      headers: headers,
      redirect: "follow",
    }

    if (method == "POST" || method == "PUT") {
      headers.append("Content-Type", "text/json")
      if (typeof body !== "undefined") requestInit.body = JSON.stringify(body)
    }

    const response = await fetch(Config.ApiBaseUrl + path, requestInit)
    console.log(response)

    if (response.status > 299) {
      if (response.status > 499) {
        alertError("Noe gikk galt")
      } else {
        let text = await response.text()
        try {
          const json = JSON.parse(text) as ApiError[]
          const errors = json.map((error: ApiError) => error.errorMessage)
          if (errors.length > 0) {
            text = errors.join("\n")
          }
        } catch {
          // Ignore
        } finally {
          alertError("Du har gjort noe feil", text)
        }
      }
      throw response
    }
    return response
  }

  return { apiRequest, apiRequestJson }
}

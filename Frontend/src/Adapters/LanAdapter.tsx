import useApiRequests from "./ApiRequestHook"
import { useApi } from "../Contexts/ApiContext"
import { useEffect, useRef } from "react"

export interface Lan {
  id: string
  guildId: string
  active: boolean
  title: string
  background: string
  createdAt: Date
  updatedAt: Date
}

export function useLanAdapter() {
  const { apiRequest } = useApiRequests()
  const { lans, setLans } = useApi()
  const isFetching = useRef<boolean>(false)

  useEffect(() => {
    if (lans == null && !isFetching.current) {
      isFetching.current = true
      reloadLans()
    }
  }, [])

  const createLan = async (
    guildId: string,
    title: string,
    background: string
  ): Promise<Response> => {
    return await apiRequest("POST", "lan", { guildId, title, background })
  }

  const updateLan = async (
    lan: Lan,
    title: string,
    background: string
  ): Promise<Response> => {
    return await apiRequest("PUT", `lan/${lan.id}`, {
      id: lan.id,
      title,
      background,
    })
  }

  const deleteLan = async (lan: Lan): Promise<Response> => {
    return await apiRequest("DELETE", `lan/${lan.id}`)
  }

  const setActiveLan = async (lan: Lan, active: boolean): Promise<Response> => {
    return await apiRequest("PUT", `lan/${lan.id}`, {
      id: lan.id,
      active,
    })
  }

  const reloadLans = async (): Promise<Lan[]> => {
    const response = await apiRequest("GET", "lan")

    const lans = (await response.json()) as Lan[]
    lans.forEach((lan) => {
      lan.createdAt = new Date(lan.createdAt)
      lan.updatedAt = new Date(lan.updatedAt)
    })

    setLans(lans)
    isFetching.current = false
    return lans
  }

  return {
    createLan,
    lans,
    deleteLan,
    updateLan,
    setActiveLan,
  }
}

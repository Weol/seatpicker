import { useEffect, useState } from "react"
import useApiRequests from "./ApiRequestHook"

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
  const { apiRequest, apiRequestJson } = useApiRequests()
  const [lans, setLans] = useState<Lan[] | null>(null)

  useEffect(() => {
    reloadLans()
  }, [])

  const createNewLan = async (
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

  const reloadLans = async (): Promise<Lan[]> => {
    const lans = await apiRequestJson<Lan[]>("GET", "lan")

    lans.forEach((lan) => {
      lan.createdAt = new Date(lan.createdAt)
      lan.updatedAt = new Date(lan.updatedAt)
    })

    setLans(lans)
    return lans
  }

  return { createNewLan, lans, reloadLans, deleteLan, updateLan }
}

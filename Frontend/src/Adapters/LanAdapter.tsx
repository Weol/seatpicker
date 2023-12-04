import { useEffect, useState } from "react"
import { useActiveGuildId } from "./ActiveGuildAdapter"
import useApiRequests from "./ApiRequestHook"
import useVersionId from "./VersionIdHook"

export type Lan = {
  id: string
  guildId: string
  active: boolean
  title: string
  background: string
  createdAt: Date
  updatedAt: Date
}

const allLansVersionName = "AllLans"

export function useLans() {
  const { apiRequest } = useApiRequests()
  const { versionId } = useVersionId(allLansVersionName)
  const [lans, setLans] = useState<Lan[] | null>(null)

  useEffect(() => {
    loadAllLans()
  }, [versionId])

  const loadAllLans = async () => {
    const response = await apiRequest("GET", "lan")

    const lans = (await response.json()) as Lan[]
    lans.forEach((lan) => {
      lan.createdAt = new Date(lan.createdAt)
      lan.updatedAt = new Date(lan.updatedAt)
    })

    setLans(lans)
  }

  return lans
}

export function useActiveLan() {
  const { apiRequest } = useApiRequests()
  const { activeGuildId } = useActiveGuildId()
  const [activeLan, setActiveLan] = useState<Lan | null>(null)

  useEffect(() => {
    loadActiveLan()
  }, [activeGuildId])

  async function loadActiveLan(): Promise<Lan | null> {
    try {
      const response = await apiRequest("GET", `lan/active?guildId=${activeGuildId}`)
      const lan = (await response.json()) as Lan

      lan.createdAt = new Date(lan.createdAt)
      lan.updatedAt = new Date(lan.updatedAt)

      setActiveLan(lan)

      return lan
    } catch (response) {
      if (response instanceof Response && response.status == 404) {
        return null
      }
      throw response
    }
  }

  return activeLan
}

export function useLanAdapter() {
  const { apiRequest } = useApiRequests()
  const { invalidate } = useVersionId(allLansVersionName)

  const createLan = async (
    guildId: string,
    title: string,
    background: string
  ): Promise<Response> => {
    const response = await apiRequest("POST", "lan", {
      guildId,
      title,
      background,
    })
    invalidate()
    return response
  }

  const updateLan = async (lan: Lan, title: string, background: string): Promise<Response> => {
    const response = await apiRequest("PUT", `lan/${lan.id}`, {
      id: lan.id,
      title,
      background,
    })
    invalidate()
    return response
  }

  const deleteLan = async (lan: Lan): Promise<Response> => {
    const response = await apiRequest("DELETE", `lan/${lan.id}`)
    invalidate()
    return response
  }

  const setActiveLan = async (lan: Lan, active: boolean): Promise<Response> => {
    const resposne = await apiRequest("PUT", `lan/${lan.id}`, {
      id: lan.id,
      active,
    })
    invalidate()
    return resposne
  }

  return {
    createLan,
    deleteLan,
    updateLan,
    setActiveLan,
  }
}

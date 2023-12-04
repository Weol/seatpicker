import { useEffect, useState } from "react"
import { selector, useRecoilValue } from "recoil"
import { default as ApiRequest, default as useApiRequests } from "./ApiRequestAdapter"
import { activeGuildIdAtom } from "./GuildAdapter"
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

const activeLanSelector = selector<Lan>({
  key: "activeLan",
  get: async ({ get }) => {
    const activeGuildId = get(activeGuildIdAtom)
    const response = await ApiRequest("GET", `lan/active?guildId=${activeGuildId}`)

    const lan = (await response.json()) as Lan

    lan.createdAt = new Date(lan.createdAt)
    lan.updatedAt = new Date(lan.updatedAt)

    return lan
  },
})

export function useLans() {
  const [lans, setLans] = useState<Lan[] | null>(null)

  useEffect(() => {
    loadAllLans()
  }, [versionId])

  const loadAllLans = async () => {
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
  const activeLan = useRecoilValue(activeLanSelector)

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

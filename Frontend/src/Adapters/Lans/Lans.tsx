import { atom, useRecoilState } from "recoil"
import ApiRequest from "../ApiRequest"

export type Lan = {
  id: string
  guildId: string
  active: boolean
  title: string
  background: string
  createdAt: Date
  updatedAt: Date
}

async function LoadAllLans() {
  const response = await ApiRequest("GET", "lan")

  const lans = (await response.json()) as Lan[]
  lans.forEach((lan) => {
    lan.createdAt = new Date(lan.createdAt)
    lan.updatedAt = new Date(lan.updatedAt)
  })

  return lans
}

export const allLansAtom = atom<Lan[]>({
  key: "allLans",
  effects: [({ setSelf }) => setSelf(LoadAllLans())],
})

export function useLans() {
  const [lans, setLans] = useRecoilState(allLansAtom)

  const createLan = async (guildId: string, title: string, background: string) => {
    await ApiRequest("POST", "lan", {
      guildId,
      title,
      background,
    })
    setLans(await LoadAllLans())
  }

  const updateLan = async (lan: Lan, title: string, background: string) => {
    await ApiRequest("PUT", `lan/${lan.id}`, {
      id: lan.id,
      title,
      background,
    })
    setLans(await LoadAllLans())
  }

  const deleteLan = async (lan: Lan) => {
    await ApiRequest("DELETE", `lan/${lan.id}`)
    setLans(await LoadAllLans())
  }

  const setActiveLan = async (lan: Lan, active: boolean) => {
    await ApiRequest("PUT", `lan/${lan.id}`, {
      id: lan.id,
      active,
    })
    setLans(await LoadAllLans())
  }

  return {
    lans,
    createLan,
    deleteLan,
    updateLan,
    setActiveLan,
  }
}

import { useLocalStorage } from "usehooks-ts"

export function useActiveGuildId() {
  const [activeGuildId, setActiveGuildId] = useLocalStorage(
    "activeGuildId",
    "654016371260260412"
  )

  return { activeGuildId, setActiveGuildId }
}

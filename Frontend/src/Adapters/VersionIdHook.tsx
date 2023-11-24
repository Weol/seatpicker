import { useLocalStorage } from "usehooks-ts"

export default function useVersionId(name: string) {
  const [versionId, setVersionId] = useLocalStorage(name, 0)

  const invalidate = () => {
    setVersionId((versionId) => versionId + 1)
  }

  return { versionId, invalidate }
}

import {useEffect, useState} from "react";
import Lan from "./Models/Lan";
import useApiRequests from "./ApiRequestHook";
import DiscordGuild from "./Models/DiscordGuild";
import {Gif} from "@mui/icons-material";

export default function useGuilds() {
  const {apiRequestJson} = useApiRequests()
  const [guilds, setGuilds] = useState<DiscordGuild[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true)

  useEffect(() => {
    reloadGuilds()
  }, [])

  const reloadGuilds = async (): Promise<DiscordGuild[]> => {
    setIsLoading(true)
    let guilds = await apiRequestJson<DiscordGuild[]>("GET", `discord/guilds`)
    setGuilds(guilds)
    setIsLoading(false)
    return guilds;
  }

  return {reloadGuilds, guilds, isLoading}
}
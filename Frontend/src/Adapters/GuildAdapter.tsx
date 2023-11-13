import { useEffect, useState } from "react";
import useApiRequests from "./ApiRequestHook";

export interface Guild {
  id: string;
  name: string;
  icon: string | null;
}

export function useGuildAdapter() {
  const { apiRequestJson } = useApiRequests();

  const [guilds, setGuilds] = useState<Guild[] | null>(null);

  useEffect(() => {
    reloadGuilds();
  }, []);

  const reloadGuilds = async (): Promise<Guild[]> => {
    const guilds = await apiRequestJson<Guild[]>("GET", `discord/guilds`);
    setGuilds(guilds);
    return guilds;
  };

  return { reloadGuilds, guilds };
}

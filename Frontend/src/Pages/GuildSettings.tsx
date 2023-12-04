/* eslint-disable @typescript-eslint/no-unused-vars */
import { Cancel, CheckBox, CheckBoxOutlineBlank } from "@mui/icons-material"
import {
  Autocomplete,
  Box,
  Button,
  Checkbox,
  Container,
  IconButton,
  Stack,
  TextField,
} from "@mui/material"
import Divider from "@mui/material/Divider"
import Typography from "@mui/material/Typography"
import { useState } from "react"
import { Role } from "../Adapters/AuthenticationAdapter"
import { Guild, GuildRole, useGuild, useGuildRoles } from "../Adapters/GuildAdapter"
import DelayedCircularProgress from "../Components/DelayedCircularProgress"
import { useAlerts } from "../Contexts/AlertContext"

export default function GuildSettings(props: { guildId: string }) {
  const { guildRoles, setGuildRoles } = useGuildRoles(props.guildId)
  const guild = useGuild(props.guildId)

  switch (guild) {
    case null:
      return <Loading />
    default:
      return guildRoles ? (
        <Loaded guild={guild} guildRoles={guildRoles} setGuildRoles={setGuildRoles} />
      ) : (
        <Loading />
      )
  }
}

function Loading() {
  return (
    <Stack width="100%" justifyContent="center" alignItems="center" sx={{ marginTop: "1em" }}>
      <DelayedCircularProgress />
    </Stack>
  )
}

function NotFound() {
  return (
    <Container>
      <Typography>No guild found</Typography>
    </Container>
  )
}

type RoleMapping = {
  id: string
  roles: Role[]
  guildRole: GuildRole | null
}

function initStagedMappings(savedGuildRoles: GuildRole[]) {
  return savedGuildRoles
    .filter((guildRole) => guildRole.roles.length > 0)
    .map((guildRole) => {
      return {
        id: crypto.randomUUID(),
        roles: guildRole.roles,
        guildRole: guildRole,
      } as RoleMapping
    })
}

function hasStagedMappingsErrors(stagedMappings: RoleMapping[]) {
  for (const stagedMapping of stagedMappings) {
    if (!stagedMapping.guildRole) return true
    if (stagedMapping.roles.length == 0) return true
  }
  return false
}

function hasUnsavedChanges(stagedMappings: RoleMapping[], savedMappings: GuildRole[]) {
  const stagedCount = stagedMappings.reduce((count, staged) => count + staged.roles.length ?? 0, 0)
  const savedCount = savedMappings.reduce((count, saved) => count + saved.roles.length ?? 0, 0)
  if (savedCount != stagedCount) return true

  for (const staged of stagedMappings) {
    const saved = savedMappings.find((saved) => saved.id == staged.guildRole?.id)
    if (!saved) return true
    for (const stagedRole of staged.roles) {
      if (!saved.roles.includes(stagedRole)) return true
    }
  }
  return false
}

function Loaded(props: {
  guild: Guild
  guildRoles: GuildRole[]
  setGuildRoles: (guild: Guild, roleMappings: GuildRole[]) => Promise<void>
}) {
  const { alertLoading, alertSuccess } = useAlerts()
  const [stagedMappings, setStagedMappings] = useState<RoleMapping[]>(
    initStagedMappings(props.guildRoles)
  )
  const hasChanges = hasUnsavedChanges(stagedMappings, props.guildRoles)
  const hasErrors = hasStagedMappingsErrors(stagedMappings)
  const saveButtondEnabled = !hasErrors && hasChanges
  const availableGuildRoles = props.guildRoles.filter(
    (guildRole) => !stagedMappings.find((staged) => staged.guildRole?.id == guildRole.id)
  )
  const addMappingButtonDisabled =
    availableGuildRoles.length == 0 || stagedMappings.length >= props.guildRoles.length

  function handleAddMappingPressed() {
    setStagedMappings([
      ...stagedMappings,
      {
        guildRole: null,
        roles: [],
        id: crypto.randomUUID(),
      },
    ])
  }

  function handleRoleMappingDeleteClick(id: string) {
    const updatedMappings = stagedMappings.filter((mapping) => mapping.id != id)

    setStagedMappings(updatedMappings)
  }

  function handleGuildRoleChange(id: string, guildRole: GuildRole | null) {
    const updatedMappings = stagedMappings.map((mapping) =>
      mapping.id == id
        ? {
            ...mapping,
            guildRole: guildRole,
          }
        : mapping
    )

    setStagedMappings(updatedMappings)
  }

  function handleRoleChange(id: string, roles: Role[]) {
    const updatedMappings = stagedMappings.map((mapping) =>
      mapping.id == id ? { ...mapping, roles: roles } : mapping
    )

    setStagedMappings(updatedMappings)
  }

  async function handleSaveClick() {
    if (!hasChanges || hasErrors) return

    const staged = stagedMappings
      .filter((staged) => staged.guildRole)
      .map((staged) => ({ ...staged.guildRole, roles: staged.roles }) as GuildRole)

    await alertLoading("Lagrer endringer...", async () => {
      await props.setGuildRoles(props.guild, staged)
    })
    alertSuccess("Endringer lagret")
  }

  const renderRoleMappingEntry = (mapping: RoleMapping) => (
    <Stack
      spacing={1}
      width={"100%"}
      direction={"row"}
      justifyContent={"space-evenly"}
      key={mapping.id}
    >
      <Autocomplete
        options={props.guildRoles}
        autoHighlight
        sx={{ width: "50%" }}
        value={mapping.guildRole}
        onChange={(e, value) => handleGuildRoleChange(mapping.id, value)}
        getOptionLabel={(option) => option.name}
        isOptionEqualToValue={(option, value) => option.id == value.id}
        getOptionDisabled={(option) =>
          !availableGuildRoles.find((guildRole) => guildRole.id == option.id)
        }
        renderOption={(props, option) => (
          <Box component="li" sx={{ color: "#" + option.color.toString(16) }} {...props}>
            {option.name}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Guild role"
            error={mapping.guildRole == null}
            inputProps={{
              ...params.inputProps,
            }}
          />
        )}
      />
      <Autocomplete
        multiple
        disableCloseOnSelect
        limitTags={1}
        options={Object.values(Role)}
        autoHighlight
        sx={{ width: "50%" }}
        value={mapping.roles}
        onChange={(e, value) => handleRoleChange(mapping.id, value)}
        getOptionLabel={(option) => option.toString()}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox
              icon={<CheckBoxOutlineBlank fontSize="small" />}
              checkedIcon={<CheckBox fontSize="small" />}
              style={{ marginRight: 8 }}
              checked={selected}
            />
            {option.toString()}
          </li>
        )}
        renderInput={(params) => {
          params.inputProps.children = undefined
          params.inputProps["aria-expanded"] = false
          params.inputProps["aria-controls"] = undefined
          params.inputProps["aria-activedescendant"] = undefined
          return <TextField {...params} label="Role" error={mapping.roles.length == 0} />
        }}
      />
      <IconButton onClick={() => handleRoleMappingDeleteClick(mapping.id)}>
        <Cancel />
      </IconButton>
    </Stack>
  )

  return (
    <Stack spacing={2} justifyContent="center" alignItems="center">
      <Stack direction={"row"} width="100%" justifyContent="space-between" alignItems="center">
        <Typography height={"100%"} variant={"h5"}>
          {props.guild.name}
        </Typography>
        <Button
          disabled={!saveButtondEnabled}
          variant={"outlined"}
          onClick={() => handleSaveClick()}
        >
          {hasChanges ? "Lagre endringer" : "Ingen endringer"}
        </Button>
      </Stack>
      <Divider sx={{ width: "100%" }} />
      <Stack spacing={2} width={"100%"} justifyContent="center" alignItems="center">
        {stagedMappings.length > 0 ? (
          stagedMappings.map((mapping) => renderRoleMappingEntry(mapping))
        ) : (
          <Typography>Ingen mappinger registrert</Typography>
        )}
        <Divider />
        <Button
          disabled={addMappingButtonDisabled}
          variant="outlined"
          onClick={handleAddMappingPressed}
        >
          Legg til mapping
        </Button>
      </Stack>
    </Stack>
  )
}

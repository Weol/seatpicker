/* eslint-disable @typescript-eslint/no-unused-vars */
import {
  Autocomplete,
  Box,
  Button,
  Divider,
  IconButton,
  Modal,
  Stack,
  TextField,
  Typography,
} from "@mui/material"
import {
  Guild,
  GuildRole,
  GuildRoleMapping,
  useGuildAdapter,
  useGuildRoles,
} from "../../Adapters/GuildAdapter"
import { Add, Cancel } from "@mui/icons-material"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import { Role } from "../../Adapters/AuthenticationAdapter"
import React, { FormEvent, useEffect, useState } from "react"
import { useAlerts } from "../../Contexts/AlertContext"

type LineEntry = {
  id: string
  guildRole: GuildRole | null
  role: Role | null
}

function Line(props: {
  roles: GuildRole[]
  role: Role | null
  guildRole: GuildRole | null
  onGuildRoleChange: (guildRole: GuildRole | null) => void
  onRoleChange: (role: Role | null) => void
  onDelete: () => void
}) {
  return (
    <Stack
      spacing={1}
      width={"100%"}
      direction={"row"}
      justifyContent={"space-evenly"}
    >
      <Autocomplete
        options={props.roles}
        autoHighlight
        sx={{ width: "50%" }}
        value={props.guildRole}
        onChange={(e, value) => props.onGuildRoleChange(value)}
        getOptionLabel={(option) => option.name}
        isOptionEqualToValue={(option, value) => option.id == value.id}
        renderOption={(props, option) => (
          <Box
            component="li"
            sx={{ color: "#" + option.color.toString(16) }}
            {...props}
          >
            {option.name}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            size="small"
            label="Guild role"
            inputProps={{
              ...params.inputProps,
            }}
          />
        )}
      />
      <Autocomplete
        options={Object.values(Role)}
        autoHighlight
        sx={{ width: "50%" }}
        value={props.role}
        onChange={(e, value) => props.onRoleChange(value)}
        getOptionLabel={(option) => option.toString()}
        renderOption={(props, option) => (
          <Box component="li" {...props}>
            {option.toString()}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            size="small"
            label="Role"
            inputProps={{
              ...params.inputProps,
            }}
          />
        )}
      />
      <IconButton onClick={() => props.onDelete()}>
        <Cancel />
      </IconButton>
    </Stack>
  )
}

export function RoleMappingModal(props: {
  guild: Guild
  open: boolean
  onClose: () => void
}) {
  const { alertError, alertLoading, alertSuccess } = useAlerts()
  const { setGuildRoleMapping } = useGuildAdapter()
  const { roles, roleMappings } = useGuildRoles(props.guild.id)
  const [stagedMappings, setStagedRoleMappings] = useState<LineEntry[] | null>(
    null
  )

  useEffect(() => {
    if (!stagedMappings && roleMappings && roles) {
      const staged = roleMappings.map((mapping) => ({
        id: crypto.randomUUID(),
        guildRole: roles.find((role) => role.id == mapping.roleId) ?? null,
        role: mapping.role,
      }))
      setStagedRoleMappings(staged)
    }
  }, [roleMappings, roles])

  function handleAddMappingPressed() {
    if (stagedMappings)
      setStagedRoleMappings([
        ...stagedMappings,
        { guildRole: null, role: null, id: crypto.randomUUID() },
      ])
  }

  function handleLineDeleted(id: string) {
    if (stagedMappings != null) {
      const updatedMappings = stagedMappings.filter(
        (mapping) => mapping.id != id
      )

      setStagedRoleMappings(updatedMappings)
    }
  }

  function handleGuildRoleChange(id: string, guildRole: GuildRole | null) {
    if (stagedMappings != null) {
      const updatedMappings = stagedMappings.map((mapping) =>
        mapping.id == id
          ? { ...mapping, guildRole: guildRole ?? null }
          : mapping
      )

      setStagedRoleMappings(updatedMappings)
    }
  }

  function handleRoleChange(id: string, role: Role | null) {
    if (stagedMappings != null) {
      const updatedMappings = stagedMappings.map((mapping) =>
        mapping.id == id ? { ...mapping, role: role ?? null } : mapping
      )

      setStagedRoleMappings(updatedMappings)
    }
  }

  async function handleSavePressed() {
    if (stagedMappings != null) {
      const staged: { roleId: string; role: Role }[] = []
      for (const mapping of stagedMappings) {
        if (mapping.guildRole != null && mapping.role != null) {
          staged.push({ role: mapping.role, roleId: mapping.guildRole.id })
        } else {
          alertError("NOPE")
          return
        }
      }

      await alertLoading("Oppdaterer mappinger", async () => {
        await setGuildRoleMapping(props.guild, staged)
      })
      alertSuccess("Mappinger er blitt oppdatert")
      props.onClose()
    }
  }

  return (
    <Modal {...props}>
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          width: ["90%", "90%", "50%"],
          bgcolor: "background.paper",
          p: 4,
        }}
      >
        <Stack spacing={2} justifyContent="center" alignItems="center">
          <Stack
            direction={"row"}
            width="100%"
            justifyContent="space-between"
            alignItems="center"
          >
            <Typography height={"100%"} variant={"h5"}>
              {props.guild.name}
            </Typography>
            <Button variant={"outlined"} onClick={handleSavePressed}>
              Lagre
            </Button>
          </Stack>
          <Divider sx={{ width: "100%" }} />
          {stagedMappings && roles ? (
            <Stack
              spacing={2}
              width={"100%"}
              justifyContent="center"
              alignItems="center"
            >
              {stagedMappings.length > 0 ? (
                stagedMappings.map((mapping) => (
                  <Line
                    key={mapping.id}
                    roles={roles}
                    role={mapping.role}
                    guildRole={mapping.guildRole}
                    onRoleChange={(role) => handleRoleChange(mapping.id, role)}
                    onGuildRoleChange={(guildRole) =>
                      handleGuildRoleChange(mapping.id, guildRole)
                    }
                    onDelete={() => handleLineDeleted(mapping.id)}
                  />
                ))
              ) : (
                <Typography>Ingen mappinger registrert</Typography>
              )}
              <Divider />
              <Button variant="outlined" onClick={handleAddMappingPressed}>
                Legg til mapping
              </Button>
            </Stack>
          ) : (
            <Stack width="100%" justifyContent="center" alignItems="center">
              <DelayedCircularProgress />
            </Stack>
          )}
        </Stack>
      </Box>
    </Modal>
  )
}

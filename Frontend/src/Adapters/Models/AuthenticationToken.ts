import {Role} from "../../Models/Role";

export default interface AuthenticationToken {
    token: string;
    expiresAt: string,
    refreshToken: string;
    userId: string;
    nick: string;
    avatar: string | null;
    roles: Role[];
};

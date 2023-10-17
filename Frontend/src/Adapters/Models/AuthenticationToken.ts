import type { Role } from './Role';

export default interface AuthenticationToken {
    token: string;
    expiresAt: Date,
    refreshToken: string;
    userId: string;
    nick: string;
    avatar: string | null;
    roles: Role[];
};

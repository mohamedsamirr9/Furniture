export interface AuthResponseDto {
  token: string;
  refreshToken: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  name: string;
  address?: string;
  profileImage?: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  userName: string;
  email: string;
  password: string;
  name: string;
  address?: string;
  nationalIdImageBase64?: string;
}

export interface RefreshTokenDto {
  token: string;
}

export interface BecomeSellerDto {
  storeName: string;
  nationalIdImageBase64?: string;
}

export interface UpdateProfileDto {
  name: string;
  address?: string;
  profileImageBase64?: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}

export interface ResetPasswordDto {
  email: string;
  token: string;
  newPassword: string;
}

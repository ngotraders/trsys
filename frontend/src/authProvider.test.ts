import { it, vi, describe, expect, beforeEach } from 'vitest'
import type { Mocked } from 'vitest'
import axios, { AxiosStatic } from 'axios'

import { authProvider } from './authProvider'

vi.mock('axios')

describe('authProvider', () => {
    beforeEach(() => {
        vi.resetAllMocks()
    })

    const mockedAxios = axios as Mocked<AxiosStatic>
    const sut = authProvider("http://test", mockedAxios);

    it('login with username and password', async () => {

        mockedAxios.post.mockResolvedValue({ status: 204 })

        const result = await sut.login({ username: 'test', password: 'password' })

        expect(axios.post).toBeCalledWith('http://test/login', {
            username: 'test',
            password: 'password',
        })

        expect(result).toEqual({
            redirectTo: '/',
            success: true
        })
    })

    it('login with email and password', async () => {

        mockedAxios.post.mockResolvedValue({ status: 204 })

        const result = await sut.login({ email: 'test@example.com', password: 'password' })

        expect(axios.post).toBeCalledWith('http://test/login', {
            username: 'test@example.com',
            password: 'password',
        })

        expect(result).toEqual({
            redirectTo: '/',
            success: true
        })
    })

    it('login with unexpected status code', async () => {

        mockedAxios.post.mockResolvedValue({ status: 200 })

        const result = await sut.login({ email: 'test@example.com', password: 'password' })

        expect(axios.post).toBeCalledWith('http://test/login', {
            username: 'test@example.com',
            password: 'password',
        })

        expect(result).toEqual({
            success: false,
            error: {
                name: "LoginError",
                message: "Invalid username or password",
            },
        })
    })

    it('login with error', async () => {

        mockedAxios.post.mockRejectedValue(new Error("Error"))

        const result = await sut.login({ email: 'test@example.com', password: 'password' })

        expect(axios.post).toBeCalledWith('http://test/login', {
            username: 'test@example.com',
            password: 'password',
        })

        expect(result).toEqual({
            success: false,
            error: {
                name: "LoginError",
                message: "Invalid username or password",
            },
        })
    })

})
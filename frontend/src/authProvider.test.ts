import { it, vi, describe, expect, beforeEach } from 'vitest'
import axios from 'axios'
import { http, HttpResponse } from 'msw'
import { authProvider } from './authProvider'
import { server } from './mocks/server'

describe('authProvider', () => {
    beforeEach(() => {
        vi.resetAllMocks()
    })

    const axiosInstance = axios.create({
        baseURL: 'http://auth/',
        withCredentials: true,
    })
    const sut = authProvider(axiosInstance);

    it('login with username and password', async () => {
        server.use(
            http.post('http://auth/login', () => new HttpResponse(null, { status: 204 }))
        )

        const result = await sut.login({ username: 'test', password: 'password' })

        expect(result).toEqual({
            redirectTo: '/',
            success: true
        })
    })

    it('login with email and password', async () => {

        server.use(
            http.post('http://auth/login', () => new HttpResponse(null, { status: 204 }))
        )

        const result = await sut.login({ email: 'test@example.com', password: 'password' })

        expect(result).toEqual({
            redirectTo: '/',
            success: true
        })
    })

    it('login with unexpected status code', async () => {

        server.use(
            http.post('http://auth/login', () => HttpResponse.json({ message: "Error" }, { status: 400 }))
        )

        const result = await sut.login({ email: 'test@example.com', password: 'password' })

        expect(result).toEqual({
            success: false,
            error: {
                name: "LoginError",
                message: "Invalid username or password",
            },
        })
    })

    it('login with error', async () => {

        server.use(
            http.post('http://auth/login', () => HttpResponse.error())
        )

        const result = await sut.login({ email: 'test@example.com', password: 'password' })

        expect(result).toEqual({
            success: false,
            error: {
                name: "LoginError",
                message: "Invalid username or password",
            },
        })
    })

})
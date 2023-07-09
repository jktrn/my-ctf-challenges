import * as http from 'http'
import * as url from 'url'
import * as fs from 'fs'

const PORT = 3000
const FLAG = 'SEKAI{TEST}'

const data = JSON.parse(fs.readFileSync('src/gacha.json', 'utf8'))
const characters = data.characters
const probabilities = [0, 0.9, 0.1, 0]

const server = http.createServer((req, res) => {
    if (req.url === undefined) {
        res.statusCode = 400
        res.setHeader('Content-Type', 'text/plain')
        res.end('Invalid URL')
        return
    }

    const reqUrl = url.parse(req.url, true)

    if (req.method === 'POST' && reqUrl.pathname === '/gacha') {
        if (req.headers['user-agent'] !== 'SekaiCTF') {
            res.statusCode = 400
            res.setHeader('Content-Type', 'text/plain')
            res.end('Invalid User-Agent')
            return
        }

        let body = ''
        req.on('data', (chunk) => {
            body += chunk.toString()
        })

        req.on('end', () => {
            const { crystals, pulls, numPulls } = JSON.parse(body)
            if (
                typeof crystals !== 'number' ||
                typeof pulls !== 'number' ||
                typeof numPulls !== 'number'
            ) {
                res.statusCode = 400
                res.setHeader('Content-Type', 'application/json')
                res.end(
                    JSON.stringify({ error: 'ERROR: Invalid request body!' })
                )
                return
            }

            const costPerPull = 100
            const totalCost = costPerPull * numPulls
            if (crystals < totalCost) {
                res.statusCode = 400
                res.setHeader('Content-Type', 'application/json')
                res.end(
                    JSON.stringify({ error: 'ERROR: Not enough crystals!' })
                )
                return
            }

            const result: { characters: any[]; flag?: string } = {
                characters: [],
            }
            for (let i = 0; i < numPulls; i++) {
                let rand = Math.random()
                let index = 0
                while (rand > probabilities[index]) {
                    rand -= probabilities[index]
                    index++
                }

                const rarity = index + 1
                const pool = characters.filter(
                    (character) => character.rarity === `${rarity}*`
                )
                const characterIndex = Math.floor(Math.random() * pool.length)
                result.characters.push(pool[characterIndex])
            }

            if (pulls === 1000000) {
                result.flag = FLAG
            }

            res.statusCode = 200
            res.setHeader('Content-Type', 'application/json')
            res.end(JSON.stringify(result))
        })
    } else {
        res.statusCode = 404
        res.setHeader('Content-Type', 'text/plain')
        res.end('Not Found')
    }
})

server.listen(PORT, () => {
    console.log(`Server listening on port ${PORT}`)
})

export default server

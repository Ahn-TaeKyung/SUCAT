mergeInto(LibraryManager.library, {
    SendData: async function(json_data) {
        const token = localStorage.getItem("accessToken");
        console.log("Retrieved token:", token);

        if (!token) {
            console.error("토큰이 없습니다. 로그인 후 다시 시도하세요.");
            return;
        }

        const tempScore = UTF8ToString(json_data);
        const jsonScore = JSON.parse(tempScore);
        const userScore = jsonScore.score;
        console.log("score:", userScore);

        var userId = 2;

        try {
            const userIdResponse = await fetch("https://sucat.shop/api/v1/game/userId", {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${token}`,
                }
            });

            const userIdData = await userIdResponse.json();
            userId = userIdData.payload;
            console.log("받은 유저 ID:", userId);

            const jsonData = JSON.stringify({
                userId: userId,
                gameCategory: "Ato",
                score: userScore,
            });
            console.log("JSON Data to send:", jsonData);

            const scoreResponse = await fetch("https://sucat.shop/api/v1/game/score", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: jsonData
            });

            const responseData = await scoreResponse.json();
            console.log("서버로부터 받은 응답:", responseData);
        } catch (error) {
            console.error("요청 중 오류 발생:", error);
        }
    }
});